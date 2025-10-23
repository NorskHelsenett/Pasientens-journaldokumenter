using Hl7.Fhir.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NorskHelsenettUpload;
using NorskHelsenettUpload.FluidExtensions;
using NorskHelsenettUpload.Ihe.Mhd.Interfaces;
using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Ihe.Mhd.Services;
using NorskHelsenettUpload.Models;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.Services;
using PatientHealthRecordsEPJ.UploadDocuments.Lists;
using PatientHealthRecordsEPJ.UploadDocuments.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;
using static Hl7.Fhir.Model.Composition;


namespace PatientHealthRecordsEPJ.Controllers;
/*
	
See project ConsoleUploadDocument for a simple example of how to upload documents to National XDS repository with Patient Health Records API, 

This controller uses the same NorskHelsenettUplaod library with fluid extensions to build the ProvideDocumentBundleRequest. 

 */

class FileDateInfo
{
	public string name { get; set; }
	public DateTime lastModified { get; set; }
}

public class UploadDocumentsController : BaseController
{
	private readonly ILogger<HomeController> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private ISession session => _httpContextAccessor.HttpContext!.Session;
	private readonly IMemoryCache _memoryCache;
	private readonly IOidcClientService _oidcClientService;
	private readonly PhrApiOptions _apiOptions;

	private readonly IPhrApiService _phrApiService;
	private readonly IBundleCreator _bundleCreator;
	private readonly OrganizationService _organizationService;

	public UploadDocumentsController(
		IOptions<SessionOptions> sessionOptions,
		ILogger<HomeController> logger, 
		IHttpContextAccessor httpContextAccessor, 
		IMemoryCache memoryCache, 
		PhrApiOptions apiOptions,
		IPhrApiService phrApiService,
		IBundleCreator bundleCreator,
		IOidcClientService oidcClientService,
		OrganizationService organizationService
		) : base(sessionOptions)
	{
		_logger = logger;
		_httpContextAccessor = httpContextAccessor;
		_memoryCache = memoryCache;
		_apiOptions = apiOptions; 
		_oidcClientService = oidcClientService;

		_phrApiService = phrApiService;
		_bundleCreator = bundleCreator;
		_organizationService = organizationService; 
	}

	private void SaveToSession(ProvideDocumentBundleRequest bundleRequest)
	{
		var sessionData = session.SessionData();
		sessionData.BundleRequest = bundleRequest;
		sessionData.SaveToSession(session);
	}

	private void SaveMetadataToSession(DocumentEntryMetadata? metaData)
	{
		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest; 

		if (request?.DocumentCollection?.SelectedDocument?.Metadata != null)
		{
			// Make sure to not overwrite authors to empty array

			metaData.Authors = request.DocumentCollection.SelectedDocument.Metadata.Authors; 

			// Save to session (including authors we already had in the session):
			request.DocumentCollection.SelectedDocument.Metadata = metaData;
		}

		sessionData.SaveToSession(session);
	}

	private void SaveSubmissionToSession(DocumentSubmissionMetadata? submission)
	{
		var sessionData = session.SessionData();

		sessionData.BundleRequest.Submission = submission;

		sessionData.SaveToSession(session);
	}

	private void SaveDocumentToSession(Document? document, string formatCode)
	{
		var sessionData = session.SessionData();
		var selectedDocument = sessionData.BundleRequest?.DocumentCollection?.SelectedDocument; 

		if (selectedDocument != null && document != null)
		{
			selectedDocument.Update(document);

			selectedDocument.Metadata.FormatCode = formatCode; 
		}

		sessionData.SaveToSession(session);
	}

	private string GetRedirectUrl()
	{
#if DEBUG
		var redirectUrl = Request.Scheme + "://" + Request.Host.Value + "/callback";
#else
		var redirectUrl = HomeController.TestRedirectUrl; 
#endif
		return redirectUrl;
	}

	public async Task<IActionResult> RefreshToken()
	{
		var redirectUrl = GetRedirectUrl(); 

		await _oidcClientService.InitializeAsync(redirectUrl); 

		var loginResult = session.SessionData();

		var result = await _oidcClientService.RefreshTokenAsync(loginResult.RefreshToken);
		
		if (result.IsError)
		{
			throw new Exception($"{result.Error}: {result.ErrorDescription}");
		}

		loginResult.AccessToken = result.AccessToken;
		loginResult.RefreshToken = result.RefreshToken;
		loginResult.SaveToSession(session); 

		return Ok(result.AccessToken);
	}

	public async Task<IActionResult> Index()
	{
		//return View();
		return RedirectToAction(nameof(Patient)); 
	}

	public async Task<IActionResult> Patient()
	{
		var sessionData = session.SessionData();

		if (string.IsNullOrEmpty(sessionData.PatientId))
		{
			return RedirectToAction("PasientValg", "Patient"); 
		}

		if (sessionData.EnvironmentId == "qa")
		{
			return View("QA"); 
		}

		if (sessionData.BundleRequest == null)
		{
			var bundleRequest = new ProvideDocumentBundleRequest();

			// Create new submission

			var submissionTitle = $"SubmissionSet-{sessionData.PatientId}-{DateTime.Now.ToString()}";

			bundleRequest.Submission
				.WithUniqueId(Guid.NewGuid())
				.WithAvailabilityStatus(DocumentSubmissionMetadata.AvailabilityStatusApproved)
				.WithSubmissionTime(DateTime.Now)
				//.WithSourceId("1.2.840.4711.815.1")
				//.WithSourceId("2.16.578.1.12.4.5.100.1")
				//.WithSourceId("2.16.578.1.12.4.5.100.15") // Source is selected on next page in wizard
				.WithPatientId(sessionData.PatientId)
				.WithTitle(submissionTitle); 				

			var orgName = sessionData.OrganizationName; 
			var orgIdentifier = sessionData.OrganizationId;
			var orgAssignOid = string.Empty;

			var hl7Organization = _organizationService.GetHl7OrganizationDetailFromOrganization(sessionData.OrgRegOrganization);

			foreach (var ident in hl7Organization.Identifier)
			{
				orgIdentifier = ident.Value;
				orgAssignOid = ident.System;
			}
			
			// Create author-entry for submission
			var author = new Author
			{
				Name = new Xcn
				{
					FamilyName = sessionData.FamilyName,
					GivenName = sessionData.GivenName,
					MiddleName = sessionData.MiddleName
				},
				InstitutionName = [
						new Xon()
				{
					Name = orgName,
					Identifier = orgIdentifier,
					AssigningAuthority = $"{orgAssignOid}"
				}]
			};

			bundleRequest.Submission.WithAuthor(author); 			

			/*.WithAuthor(new Author()
				{
					Name = new NorskHelsenettUpload.Models.Xcn()
					{
						PersonIdentifier = sessionData.PersonId,
						GivenName = sessionData.Name,
						FamilyName = sessionData.Name,
						//MiddleName = sessionData.Name,
						//Prefix
						//Suffix	
						//Degree
					}

				})
				; 
			*/


			sessionData.BundleRequest = bundleRequest; 
		}

		var request = sessionData.BundleRequest; 

		if (request.Patient == null)
		{
			request.Patient = sessionData
				.SourcePatientInfo!
				.WithXdsGlobalIdentifier("1.2.3") // If we fix/change this, also fix in PatientController
				.WithXdsHomeCommunity("1.2.3")
				;



			/*var s = sessionData.PatientName!.Split(" ");
			var firstName = s[0];
			var middleName = string.Empty; 
			var lastName = string.Empty;

			if (s.Length == 2)
			{
				lastName = s[1];
			}
			else if (s.Length >= 3)
			{
				middleName = s[1];
				lastName = s[2];
			} */

			/*
			 
			request.Patient = new SourcePatientInfo();
			var firstName = "Malin";
			var middleName = string.Empty;
			var lastName = "Fos Bogen";

			request.Patient
				.WithName(new PersonName() 
				{ 
					FirstName = firstName, 
					MiddleName = middleName, 
					FamilyName = lastName 
				})
				.WithNIN(sessionData.PatientId!, NinType.NinTypeNationalIdentityNumber)
				.WithXdsGlobalIdentifier("1.2.3")
				.WithXdsHomeCommunity("1.2.3")
				.WithGender("F")
				.WithDateOfBirth(new DateTime(1969, 11, 13));  */


			sessionData.SaveToSession(session); 
		}

		return View("Steps/Patient");
	}

	[HttpPost]
	public async Task<IActionResult> Patient(NavigationAction navButton)
	{
		if (navButton == NavigationAction.Previous)
		{			
		}

		if (navButton == NavigationAction.Next)
		{
			//return RedirectToAction(nameof(UploadDocument));
			return RedirectToAction(nameof(Source));
		}

		return View(); 
	}

	public async Task<IActionResult> Source()
	{
		var sessionData = session.SessionData();
		
		sessionData.SaveToSession(session);

		return View("Steps/Source");
	}

	[HttpPost]
	public async Task<IActionResult> Source(string selectedHomeCommunityId, NavigationAction navButton)
	{
		if (!string.IsNullOrEmpty(selectedHomeCommunityId))
		{
			navButton = NavigationAction.Next; 
		}

		if (navButton == NavigationAction.Previous)
		{
			return RedirectToAction(nameof(Patient));
		}		

		if (!string.IsNullOrEmpty(selectedHomeCommunityId))
		{
			var sessionData = session.SessionData();
			var request = sessionData.BundleRequest;

			request!.Submission.WithSourceId(selectedHomeCommunityId);

			sessionData.SaveToSession(session);
		}

		if (navButton == NavigationAction.Next)
		{
			return RedirectToAction(nameof(UploadDocument));
		}

		return View();
	}

	public async Task<IActionResult> UploadDocument()
	{
		var model = new DocumentMetadataViewModel();

		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest;

		model.SelectedDocument = request.DocumentCollection?.SelectedDocument; 
		
		model.SelectMimeTypeList = new SelectList(
			MimeTypeList.MimeTypes,
			nameof(TextValue.Value),
			nameof(TextValue.Text)
		);

		model.SelectCompositionFormatList = new SelectList(
			CompositionFormatList.CompositionFormats,
			nameof(TextValue.Value),
			nameof(TextValue.Text)
		);

		model.SelectLanguageList = new SelectList(
			LanguageList.Languages,
			nameof(TextValue.Value),
			nameof(TextValue.Text)
		);

		return View("Steps/UploadDocument", model);
	}

	[HttpPost]
	public async Task<IActionResult> UploadDocument(DocumentMetadataViewModel model, string? selectedFileUniqueId, NavigationAction navButton)
	{
		var sessionData = session.SessionData();

		if (navButton == NavigationAction.Previous)
		{
			return RedirectToAction(nameof(Source));
		}		

		SaveDocumentToSession(model.SelectedDocument, model.SelectedDocument!.Metadata.FormatCode!);

		if (!string.IsNullOrEmpty(selectedFileUniqueId))
		{
			sessionData = session.SessionData(); // Reload since it has been overwritten in session above in SaveMetadataToSession

			Guid? fileUniqueId;

			if (selectedFileUniqueId == "remove_all")
			{
				sessionData.BundleRequest?.DocumentCollection.Clear(); 
			}
			else if (selectedFileUniqueId.StartsWith("remove"))
			{
				fileUniqueId = new Guid(selectedFileUniqueId.Replace("remove,", "").Trim());
				sessionData.BundleRequest?.DocumentCollection.RemoveDocument(fileUniqueId.Value);
			}
			else
			{
				fileUniqueId = new Guid(selectedFileUniqueId);

				sessionData.BundleRequest?.DocumentCollection.SelectDocument(fileUniqueId.Value);
			}

			sessionData.SaveToSession(session);

			return RedirectToAction(nameof(UploadDocument));
		}

		if (navButton == NavigationAction.Next)
		{		
			return RedirectToAction(nameof(Classification));
		}

		return View();
	}

	[HttpPost]
	public async Task<IActionResult> UploadFiles(List<IFormFile> files, string fileCreationDates, string dateType)
	{	
		if (files == null || files.Count == 0)
			return BadRequest("No files uploaded.");

		if (string.IsNullOrWhiteSpace(fileCreationDates))
			return BadRequest("No file creation dates provided.");

		// Deserialize JSON
		var fileDateList = JsonSerializer.Deserialize<List<FileDateInfo>>(fileCreationDates);

		if (fileDateList == null || fileDateList.Count != files.Count)
			return BadRequest("Mismatch between files and creation dates.");

		for (int i = 0; i < files.Count; i++)
		{
			var file = files[i];
			var fileDate = fileDateList.FirstOrDefault(f => f.name == file.FileName);

			if (file == null || file.Length == 0)
				return BadRequest("No file uploaded.");

			var sessionData = session.SessionData();
			var request = sessionData.BundleRequest;

			byte[] bytes;
			using (var ms = new MemoryStream())
			{
				await file.CopyToAsync(ms);
				bytes = ms.ToArray();
			}

			DateTime creationTime = fileDate!.lastModified;

			var extension = Path.GetExtension(file.FileName);

			if (extension.ToLower() != ".xml")
			{
				switch (dateType)
				{
					// Use current date and time as document date
					case "now":
						creationTime = DateTime.Now;
						break;

					// Use file date as document date
					case "filedate":
						creationTime = fileDate!.lastModified;
						break;

					default:
						break;
				}
			}

			var document = await CreateDocument(file.FileName, file.ContentType, bytes, creationTime);

			request.DocumentCollection.WithDocument(document, selectDocument: true);

			sessionData.SaveToSession(session);
		}

		//return View("Steps/UploadDocument");
		return RedirectToAction(nameof(UploadDocument));
	}

	private async Task<Document> CreateDocument(string fileName, string contentType, byte[] content, DateTime lastModified, bool createRandom = false)
	{
		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest;

		var extension = Path.GetExtension(fileName);

		string mimeType = NormalizeMime(contentType, extension);

		var formatCode = CompositionFormatDetector.DetermineFormat(content, ref mimeType, fileName);

		Document document = new Document()
			//Document document = upload.DocumentCollection.SelectedDocument		
			.WithUniqueId(Guid.NewGuid())
			.WithFilename(fileName)
			.WithContent(content, computeSha: true)
			.WithMimeType(mimeType)
			//.WithCreationTime(fileDate!.lastModified)
			.WithCreationTime(lastModified)
			//.WithSize((int)file.Length);        // TODO: make .Size property of type long? 
			.WithSize((int)content.Length);        // TODO: make .Size property of type long? 

		//.WithLanguageCode("nb-NO");		// TODO: detect automatically? 

		// Create an author. For now we'll just use the logged in health personell. 

		var orgName = sessionData.OrganizationName;
		var orgIdentifier = sessionData.OrganizationId;
		var orgAssignOid = string.Empty;

		var hl7Organization = _organizationService.GetHl7OrganizationDetailFromOrganization(sessionData.OrgRegOrganization);

		foreach (var ident in hl7Organization.Identifier)
		{
			orgIdentifier = ident.Value;
			orgAssignOid = ident.System;
		}

		document.Metadata
			.WithPatientId(sessionData.PatientId!)
			//.WithSourcePatientId(sessionData.PatientId!)
			.WithSourcePatientIdToHl7String(request.Patient.NIN, NinType.NinTypeNationalIdentityNumber)
			.WithFormatCode(formatCode)
			.WithServiceStartTime(DateTime.Now.Subtract(TimeSpan.FromDays(7)))
			.WithServiceStopTime(DateTime.Now.Subtract(TimeSpan.FromDays(6)))
			.WithAuthor(new Author()
			{
				Name = new Xcn()
				{
					GivenName = sessionData.GivenName,
					FamilyName = sessionData.FamilyName,
					MiddleName = sessionData.MiddleName
				},
				InstitutionName = [
				new Xon()
				{
					Name = orgName,
					Identifier = orgIdentifier,
					AssigningAuthority = $"{orgAssignOid}"
				}]
			})
			;

		/*document.Metadata.SourcePatientInfo!
			.WithName(new PersonName() 
			{ 
				//FirstName = sessionData.PatientName, 
				FirstName = "Malin",
				MiddleName = null, 
				FamilyName = "Fos Bogen" 
			})
			.WithNIN(sessionData.PersonId, NinType.NinTypeNationalIdentityNumber)
			.WithXdsGlobalIdentifier("1.2.3")
			.WithXdsHomeCommunity("1.2.3")
			.WithGender("F")
			.WithDateOfBirth(new DateTime(1969, 11, 13)) 
			; 
		*/
		//.WithDateOfBirth
		//.WithGender			
		//WithXdsGlobalIdentifier
		//WithXdsHomeCommunity


		switch (extension)
		{
			case ".xml":
				XmlDocument doc = new XmlDocument();
				//doc.Load(file.OpenReadStream());

				using (var stream = new MemoryStream(content))
				{
					doc.Load(stream);
				}

				var test = doc.DocumentElement.OuterXml;

				if (doc.DocumentElement.Name == "ClinicalDocument")
				{
					// TODO: in a real application you could properly deserialize the clinical document. 
					// As an example we just read the base64 encoded PDF string, or the KITH xml string

					var namespaceManager = new XmlNamespaceManager(doc.NameTable);
					namespaceManager.AddNamespace("hl7", doc.DocumentElement!.NamespaceURI);

					var realmNode = doc.DocumentElement!.SelectSingleNode("//hl7:realmCode", namespaceManager);
					var typeIdNode = doc.DocumentElement!.SelectSingleNode("//hl7:typeId", namespaceManager);
					var templateIdNode = doc.DocumentElement!.SelectSingleNode("//hl7:templateId", namespaceManager);
					var idNode = doc.DocumentElement!.SelectSingleNode("//hl7:id", namespaceManager);
					var codeNode = doc.DocumentElement!.SelectSingleNode("//hl7:code", namespaceManager);   // this is the typeCode (child category), and we determine top-level classCode from it (top level category) 
					var titleNode = doc.DocumentElement!.SelectSingleNode("//hl7:title", namespaceManager);
					var effectiveTimeNode = doc.DocumentElement!.SelectSingleNode("//hl7:effectiveTime ", namespaceManager);
					var confidentialityCodeNode = doc.DocumentElement!.SelectSingleNode("//hl7:confidentialityCode", namespaceManager);
					var languageCodeNode = doc.DocumentElement!.SelectSingleNode("//hl7:languageCode", namespaceManager);
					var setIdNode = doc.DocumentElement!.SelectSingleNode("//hl7:setId", namespaceManager);
					var recordTargetNode = doc.DocumentElement!.SelectSingleNode("//hl7:recordTarget", namespaceManager);
					var authorNode = doc.DocumentElement!.SelectSingleNode("//hl7:author", namespaceManager);
					var custodianNode = doc.DocumentElement!.SelectSingleNode("//hl7:custodian", namespaceManager);
					var informationRecipientNode = doc.DocumentElement!.SelectSingleNode("//hl7:informationRecipient", namespaceManager);

					var textNode = doc.DocumentElement!.SelectSingleNode("//hl7:component/hl7:nonXMLBody/hl7:text", namespaceManager);

					if (titleNode != null)
					{
						document.WithTitle(titleNode.InnerText);
					}

					if (codeNode != null)
					{
						// Set typeCode (child level) 
						var typeCode = codeNode.Attributes?["code"]?.Value!;
						var typeCodeSystem = codeNode.Attributes?["codeSystem"]?.Value!;

						var typeCodeCoding = TypeCodeList.TypeCodes.FirstOrDefault(coding => coding.Code == typeCode);

						document.Metadata.WithTypeCode(typeCodeCoding);

						// Set classCode (top-level) 

						var firstLetter = typeCode[0];

						var classCodeCoding = ClassCodeList.ClassCodes.FirstOrDefault(coding => coding.Code.StartsWith(firstLetter));

						if (classCodeCoding != null)
						{
							document.Metadata.WithClassCode(classCodeCoding);
						}
					}

					if (confidentialityCodeNode != null)
					{
						var confidentialityCode = confidentialityCodeNode.Attributes?["code"]?.Value;
						var confidentialityCodeSystem = confidentialityCodeNode.Attributes?["codeSystem"]?.Value;

						var confidentialityCodeCoding = ConfidentialityCodeList.ConfidentialityCodes.FirstOrDefault(coding => coding.Code == confidentialityCode);

						document.Metadata.WithConfidentialityCode(confidentialityCodeCoding);
					}

					if (languageCodeNode != null)
					{
						document.WithLanguageCode(languageCodeNode.Attributes?["code"]?.Value);
					}

					if (textNode is not null)
					{
						var mediaType = textNode.Attributes["mediaType"]?.Value;
						var representation = textNode.Attributes["representation"]?.Value;

						if (mediaType == "application/pdf" && representation == "B64")
						{
							//model.PdfBase64String = textNode.InnerText;
							//model.ContentType = "application/pdf";

							// Browsers have trouble displaying large PDF files properly when using base64 in the 'src' so create another solution with a
							// "DownloadPdf" action method and point the IFRAME src to that instead? 
							//var pdfBytes = Convert.FromBase64String(model.PdfBase64String); // With this I have verified that PDF files are in the correct format even though some can't currently be shown in the browser 

							textNode.InnerText = "...removed for display...";

							//model.ClinicalDocumentXml = PrettyPrintXml(doc.InnerXml);
							//model.IsShowClinicalDocumentXml = true;

							//model.IsSuccess = true;

							//return model;
						}
						else if (mediaType == "application/xml")
						{
							// Extract KITH xml from ClinicalDocument

							//xml = string.Empty;

							//xml = representation == "B64" ? Encoding.UTF8.GetString(Convert.FromBase64String(textNode.InnerText)) : textNode.InnerText;

							//doc.LoadXml(xml);

							// Fall through to displaying KITH xml below
						}
					}
				}
				break;

			default:
				var fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);
				
				document.WithTitle(ToPascalCase(fileNameNoExtension));

				var confidentiality = ConfidentialityCodeList.ConfidentialityCodes.FirstOrDefault(coding => coding.Code == "N");

				document.Metadata.WithConfidentialityCode(confidentiality);
			
				break;
		}

		if (createRandom)
		{
			// Healthcare facility type and practice setting code cannot (currently?) be read from CDA
			// TODO: generate more data randomly? 
			
			var healthCareFacilityType = HealthcareFacilityTypeList.HealthcareFacilityTypes[Random.Shared.Next(HealthcareFacilityTypeList.HealthcareFacilityTypes.Count)];
			
			var practiceSettingCode = PracticeSettingCodeList.PracticeSettingCodes[Random.Shared.Next(PracticeSettingCodeList.PracticeSettingCodes.Count)];

			document.Metadata.WithHealthCareFacilityType(healthCareFacilityType);
			document.Metadata.WithPracticeSettingCode(practiceSettingCode); 
		}
		return document; 
	}

	private static string ToPascalCase(string s)
	{
		if (string.IsNullOrWhiteSpace(s))
			return string.Empty;

		// Replace separators (underscore, dash, or space) with a single space
		s = Regex.Replace(s, @"[_\-\s]+", " ");

		// Split into words, capitalize each, then concatenate
		TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
		var words = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		for (int i = 0; i < words.Length; i++)
		{
			words[i] = textInfo.ToTitleCase(words[i].ToLowerInvariant());
		}

		return string.Concat(words);
	}

	private static string NormalizeMime(string contentType, string extension)
	{
		if (string.IsNullOrWhiteSpace(contentType) || contentType == "application/octet-stream")
		{
			// simple extension-based normalization
			return extension?.ToLowerInvariant() switch
			{
				".pdf" => "application/pdf",
				".xml" => "application/xml",
				".rtf" => "application/rtf",
				".txt" => "text/plain",
				_ => contentType ?? "application/octet-stream"
			};
		}

		// Map a couple of known oddities
		return contentType switch
		{
			"text/rtf" => "application/rtf",
			"application/msword" => "application/rtf", // many scanners send RTF as msword
			_ => contentType
		};
	}
	
	[HttpPost]
	public async Task<IActionResult> GenerateRandomTestDocuments(int entriesToGenerate)
	{
		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest;

		var testDataFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "TestData"));
		var testDataDocuments = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "TestData", "Documents"));

		using var testDataFile = System.IO.File.OpenText(testDataFiles.First(f => f.Contains("TestDataRegistryObjects.json")));

		var jsonTestData = RegistryJsonSerializer.Deserialize<Test_DocumentReference>(await testDataFile.ReadToEndAsync());
		if (jsonTestData == null)
		{
			return null;
		}

		jsonTestData.PossibleSubmissionSetValues.Authors = jsonTestData.PossibleDocumentEntryValues.Authors;

		var files = testDataDocuments.Select(file => System.IO.File.ReadAllBytes(file)).ToList();

		var creationTime = DateTime.Now;

		var generatedTestRegistryObjects = TestDataGeneratorService.GenerateRegistryObjectsFromTestData(jsonTestData, entriesToGenerate, creationTime);

		var files2 = jsonTestData.Documents.Select(file => Convert.FromBase64String(file));
		
		var sxmls = new SoapXmlSerializer(XcaXds.Commons.Commons.Constants.XmlDefaultOptions.DefaultXmlWriterSettings);

		foreach (var generatedTestObject in generatedTestRegistryObjects.OfType<DocumentEntryDto>())
		{			
			var randomFileAsByteArray = files.ElementAt(Random.Shared.Next(files.Count()));

			if (generatedTestObject?.SourcePatientInfo?.PatientId?.Id != null && generatedTestObject.Id != null && randomFileAsByteArray != null)
			{
				generatedTestObject.Title = "XcaDS - " + generatedTestObject.Title;
				generatedTestObject.Size = randomFileAsByteArray.Length.ToString();
				using (var md5 = MD5.Create())
				{
					generatedTestObject.Hash = BitConverter.ToString(md5.ComputeHash(randomFileAsByteArray)).Replace("-", "");
				}

				//generatedTestObject.RepositoryUniqueId = _xdsConfig.RepositoryUniqueId;
				//generatedTestObject.HomeCommunityId = _xdsConfig.HomeCommunityId;

				generatedTestObject.RepositoryUniqueId = request!.Submission.SourceId + ".25"; // TODO: fix. And don't use .25 because that's just a testing default 
				generatedTestObject.HomeCommunityId = request.Submission.SourceId; 

				//_repositoryWrapper.StoreDocument(generatedTestObject.UniqueId, randomFileAsByteArray, generatedTestObject.SourcePatientInfo.PatientId.Id);

				var submissionSet = generatedTestRegistryObjects
				.OfType<SubmissionSetDto>()
				.FirstOrDefault(ro => ro.Id == generatedTestRegistryObjects?
					.OfType<AssociationDto>()?
					.FirstOrDefault(assoc => assoc.TargetObject == generatedTestObject?.Id)?.SourceObject);

				var documentDto = new DocumentDto()
				{
					// Data = _repository.Read(documentUniqueId),
					Data = randomFileAsByteArray,
					DocumentId = generatedTestObject?.Id
				};

				var clinicalDocument = CdaTransformerService.TransformRegistryObjectsToClinicalDocument(generatedTestObject, submissionSet, documentDto);
				var cdaXml = sxmls.SerializeSoapMessageToXmlString(clinicalDocument, XcaXds.Commons.Commons.Constants.XmlDefaultOptions.DefaultXmlWriterSettingsInline).Content;

				var cdaXmlBytes = Encoding.UTF8.GetBytes(cdaXml);

				var contentType = "application/xml";
				var fileName = $"CDA_{generatedTestObject.Id}.xml"; 

				var document = await CreateDocument(
					//generatedTestObject.Title,
					//generatedTestObject.MimeType!,
					fileName,
					contentType,
					cdaXmlBytes,
					generatedTestObject.CreationTime!.Value,					
					createRandom: true
					);

				request.DocumentCollection.WithDocument(document, selectDocument: true);

				sessionData.SaveToSession(session);

			}

			/*var cdaDoc = CdaBuilder.FromDocumentEntryDto(generatedTestObject);
			var outputPath = Path.Combine(AppContext.BaseDirectory, "GeneratedCda", $"CDA_{generatedTestObject.Id}.xml");

			Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
			cdaDoc.Save(outputPath); */

			
		}

		//_registryWrapper.UpdateDocumentRegistryContentWithDtos(generatedTestRegistryObjects);

		return RedirectToAction(nameof(UploadDocument));		
	}
	
	public async Task<IActionResult> Classification()
	{
		var model = new ClassificationViewModel(); 

		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest;

		model.SelectedDocument = request.DocumentCollection?.SelectedDocument;

		//model.Upload = request;
		model.Metadata = request.DocumentCollection?.SelectedDocument?.Metadata;
	
		
		//upload.Classification.

		//
		//	Encounter information
		//		

		model.SelectHealthcareFacilityList = new SelectList(			
			HealthcareFacilityTypeList.HealthcareFacilityTypes.Select(c => 
			new TextValue(
				$"{c.System}|{c.Code}|{c.Display}",	// Value in select list
				$"{c.Display} [{c.Code}]"			// What the user sees
			)),
			nameof(TextValue.Value),				// Name of property used for value
			nameof(TextValue.Text)                  // Name of property used for text
		);
		
		model.SelectPracticeSettingCodeList = new SelectList(
			PracticeSettingCodeList.PracticeSettingCodes.Select(c =>
			new TextValue(
				$"{c.System}|{c.Code}|{c.Display}",
				$"{c.Display} [{c.Code}]"
			)),
			nameof(TextValue.Value),
			nameof(TextValue.Text)
		);

		//
		//	Document classification
		//

		model.SelectClassificationList = new SelectList(
			ClassCodeList.ClassCodes.Select(c =>
			new TextValue(
				$"{c.System}|{c.Code}|{c.Display}",
				$"{c.Display} [{c.Code}]"
			)),
			nameof(TextValue.Value),
			nameof(TextValue.Text)
		);

		model.SelectCategoryTypeList = new SelectList(
			TypeCodeList.TypeCodes.Select(c =>
			new TextValue(
				$"{c.System}|{c.Code}|{c.Display}",
				$"{c.Display} [{c.Code}]"
			)),
			nameof(TextValue.Value),
			nameof(TextValue.Text)
		);

		model.SelectConfidentialityCodeList = new SelectList(
			ConfidentialityCodeList.ConfidentialityCodes.Select(c =>
			new TextValue(
				$"{c.System}|{c.Code}|{c.Display}",
				$"{c.Display} [{c.Code}]"
			)),
			nameof(TextValue.Value),
			nameof(TextValue.Text)
		);

		return View("Steps/Classification", model);
	}

	[HttpPost]
	public async Task<IActionResult> Classification(ClassificationViewModel model, string? selectedFileUniqueId, NavigationAction navButton)
	{
		var sessionData = session.SessionData();

		SaveMetadataToSession(model.Metadata);

		if (!string.IsNullOrEmpty(selectedFileUniqueId))
		{
			sessionData = session.SessionData(); // Reload since it has been overwritten in session above in SaveMetadataToSession

			Guid? fileUniqueId;
			
			if (selectedFileUniqueId == "remove_all")
			{
				sessionData.BundleRequest?.DocumentCollection.Clear();
			}
			else if (selectedFileUniqueId.StartsWith("remove"))
			{
				fileUniqueId = new Guid(selectedFileUniqueId.Replace("remove,", "").Trim());
				sessionData.BundleRequest?.DocumentCollection.RemoveDocument(fileUniqueId.Value);
			}
			else
			{
				fileUniqueId = new Guid(selectedFileUniqueId);

				sessionData.BundleRequest?.DocumentCollection.SelectDocument(fileUniqueId.Value);
			}

			sessionData.SaveToSession(session);

			return RedirectToAction(nameof(Classification));
		}

		if (navButton == NavigationAction.Previous)
		{
			return RedirectToAction(nameof(UploadDocument));
		}

		if (navButton == NavigationAction.Next)
		{
			return RedirectToAction(nameof(Submission));
		}


		return View(); 
	}	

	public async Task<IActionResult> Submission()
	{
		var model = new SubmissionViewModel();

		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest;

		model.Submission = request.Submission;

		var author = request.Submission.Author.First(); 
		
		model.AuthorName = author.Name.GivenName + (!string.IsNullOrEmpty(author.Name.MiddleName) ? $" {author.Name.MiddleName}" : "") + $" {author.Name.FamilyName}"; 

		model.SelectAvailabilityStatusList = new SelectList(
			AvailabilityStatusList.AvailabilityStatuses,
			nameof(TextValue.Value),      // Value
			nameof(TextValue.Text)    // Text
		);
		
		return View("Steps/Submission", model);
	}

	[HttpPost]
	public async Task<IActionResult> Submission(SubmissionViewModel model, NavigationAction navButton)
	{
		var nameParts = model.AuthorName.Split(" ");

		var authorFirstName = nameParts[0];
		var authorLastName = nameParts[1];
		
		var sessionData = session.SessionData();

		var author = sessionData.BundleRequest.Submission.Author.First();

		author.Name.GivenName = authorFirstName;
		author.Name.FamilyName = authorLastName;

		model.Submission.WithAuthor(author); 

		/*var author = model.Submission.Author.First();

		author.Name.GivenName = authorFirstName;		
		author.Name.FamilyName = authorLastName;*/

		// Create author-entry for submission
		/*var author = new Author
		{
			Name = new Xcn
			{
				FamilyName = authorLastName,
				GivenName = authorFirstName,
				MiddleName = ""
			},
			InstitutionName = [
					new Xon()
				{
					Name = orgName,
					Identifier = orgIdentifier,
					AssigningAuthority = $"{orgAssignOid}"
				}]
		};

		bundleRequest.Submission.WithAuthor(author); */


		SaveSubmissionToSession(model.Submission);

		if (navButton == NavigationAction.Previous)
		{
			return RedirectToAction(nameof(Classification));
		}

		if (navButton == NavigationAction.Next)
		{
			return RedirectToAction(nameof(FinalizeSubmission));
		}

		return View();
	}

	public async Task<IActionResult> FinalizeSubmission(string error)
	{
		var model = new FinalizeSubmissionViewModel();

		model.ErrorMessage = error;
		model.IsError = !string.IsNullOrEmpty(error); 

		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest;

		model.Upload = request;

		model.JsonBundleDocument = _bundleCreator.CreateFhirBundle(request, "Comprehensive", removeBinaryDataForDisplay: true);

		// Replace the base64 encoded embedded data with just text "...removed for display..." (only done for documents with index > 0 if there are more than 1 document)
		model.JsonBundleDocument = model.JsonBundleDocument.Replace(BundleCreator.REMOVED_FOR_DISPLAY_TEXT_BASE64, "...removed for display...");

		return View("Steps/FinalizeSubmission", model);
	}

	public IActionResult ViewJsonFhirBundle()
	{	
		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest;		

		var bundleDocumentJsonString = _bundleCreator.CreateFhirBundle(request, "Comprehensive", removeBinaryDataForDisplay: true);

		// Replace the base64 encoded embedded data with just text "...removed for display..." (only done for documents with index > 0 if there are more than 1 document)
		bundleDocumentJsonString = bundleDocumentJsonString.Replace(BundleCreator.REMOVED_FOR_DISPLAY_TEXT_BASE64, "...removed for display..."); 

		return Content(bundleDocumentJsonString, "application/json"); 
	}
		
	[HttpPost]
	public async Task<IActionResult> FinalizeSubmission(FinalizeSubmissionViewModel model, NavigationAction navButton)
	{
		if (navButton == NavigationAction.Previous)
		{
			return RedirectToAction(nameof(Submission));
		}

		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest;

		// Create document-object
		//var bundleDocument = _bundleCreator.CreateFhirBundle(Model, "Comprehensive");
		var bundleDocument = _bundleCreator.CreateFhirBundle(request, "Comprehensive");

		var apiResult = await _phrApiService.ProvideDocumentBundleAsync(sessionData.AccessToken, Constants.JwkPrivateKey, bundleDocument);
		//await _phrApiService.CreateDocumentReferenceAsync(bundleDocument); 

		if (apiResult.IsSuccess)
		{
			return RedirectToAction(nameof(SubmissionConfirmation));
		}
		else
		{
			return RedirectToAction(nameof(FinalizeSubmission), new { error = "An error occurred trying to register document bundle"});
		}		
	}	
	public async Task<IActionResult> SubmissionConfirmation()
	{
		var model = new FinalizeSubmissionViewModel();

		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest;

		model.Upload = request;

		sessionData.BundleRequest = null;
		sessionData.UploadWizardOpenSteps.Clear();
		sessionData.SaveToSession(session); 

		return View("Steps/SubmissionConfirmation", model);
	}

	[HttpPost]
	public async Task<IActionResult> SubmissionConfirmation(FinalizeSubmissionViewModel model, NavigationAction navButton)
	{
		if (navButton == NavigationAction.Previous)
		{
			return RedirectToAction(nameof(Submission));
		}

		if (navButton == NavigationAction.Next)
		{
			//return RedirectToAction(nameof(Confirm));
		}

		return View();
	}
}
