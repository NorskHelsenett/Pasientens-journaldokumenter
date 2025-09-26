using Hl7.Fhir.Model.CdsHooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NorskHelsenettUpload;
using NorskHelsenettUpload.FluidExtensions;
using NorskHelsenettUpload.Ihe.Mhd.Interfaces;
using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Models;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.Services;
using PatientHealthRecordsEPJ.UploadDocuments;
using PatientHealthRecordsEPJ.UploadDocuments.Lists;
using PatientHealthRecordsEPJ.UploadDocuments.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Xml;


namespace PatientHealthRecordsEPJ.Controllers;
/*
	
For a simple example of how to upload documents to National XDS repository with Patient Health Records API, see project ConsoleUploadDocument. 

This controller uses the same NorskHelsenettUplaod library with fluid extensions to build the ProvideDocumentBundleRequest. 

 */
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
				.WithSourceId("2.16.578.1.12.4.5.100.1")
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

			/*
			var givenName = user.Claims.Where(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname").First().Value;
			var middleName = user.Claims.Where(x => x.Type == "middle_name").First().Value;
			var familyName = user.Claims.Where(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname").First().Value;
			authorName = $"{givenName}{" " + middleName} {familyName}";

			// Retrieve submission's AUTHOR-ORG details
			var org = new Hl7.Fhir.Model.Organization();


			try
			{
				// Get token
				var result = await ApiService.GetTokenAsync();
				var jwtToken = JsonSerializer.Deserialize<AccessTokenWithExpiration>(result);
				var handler = new JwtSecurityTokenHandler();
				var token = handler.ReadJwtToken(jwtToken!.accessToken);

				var claimClientOrgNr = token.Claims.Where(x => x.Type == "helseid://claims/client/claims/orgnr_parent").First()!.Value;

				org = await ApiService.GetOrganizationDetailsAsync(claimClientOrgNr);

				orgName = org.Name;

				foreach (var ident in org.Identifier)
				{
					orgIdentifier = ident.Value;
					orgAssignOid = ident.System;
				}
			}
			catch
			{
				//Console.Error.WriteLine("Problems while retrieving organization's details from OrgReg [Grunndata]");
			} */

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
				.WithXdsGlobalIdentifier("1.2.3")
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
		//model.Metadata = request.DocumentCollection?.SelectedDocument?.Metadata;

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
	public async Task<IActionResult> UploadDocument(DocumentMetadataViewModel model, NavigationAction navButton)
	{
		//SaveToSession(model.Metadata);
		SaveDocumentToSession(model.SelectedDocument, model.SelectedDocument!.Metadata.FormatCode!); 

		if (navButton == NavigationAction.Previous)
		{
			return RedirectToAction(nameof(Patient));
		}

		if (navButton == NavigationAction.Next)
		{
			return RedirectToAction(nameof(Classification));
		}

		return View();
	}

	[HttpPost]
	public async Task<IActionResult> UploadFiles(IFormFile file, DateTime fileCreationDate)
	{
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

		var extension = Path.GetExtension(file.FileName);

		string mimeType = NormalizeMime(file.ContentType, extension);

		var formatCode = CompositionFormatDetector.DetermineFormat(bytes, ref mimeType, file.FileName);

		Document document = new Document()
		//Document document = upload.DocumentCollection.SelectedDocument		
			.WithUniqueId(Guid.NewGuid())			
			.WithFilename(file.FileName)
			.WithContent(bytes, computeSha: true)
			.WithMimeType(mimeType)
			.WithCreationTime(fileCreationDate)			
			.WithSize((int)file.Length);        // TODO: make .Size property of type long? 

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
				doc.Load(file.OpenReadStream());

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
				break;
		}

		request.DocumentCollection.WithDocument(document, selectDocument: true);

		sessionData.SaveToSession(session);

		//return View("Steps/UploadDocument");
		return RedirectToAction(nameof(UploadDocument));
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

	public async Task<IActionResult> Classification()
	{
		var model = new ClassificationViewModel(); 

		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest;

		//model.Upload = request;
		model.Metadata = request.DocumentCollection?.SelectedDocument?.Metadata;

		//upload.Classification.

		//
		//	Encounter information
		//

		/*model.SelectHealthcareFacilityList = new SelectList(
			HealthcareFacilityTypeList.HealthcareFacilityTypes,
			nameof(SimpleCoding.Code),		// Value
			nameof(SimpleCoding.Display)	// Text
		);*/

		model.SelectHealthcareFacilityList = new SelectList(
			//HealthcareFacilityTypeList.HealthcareFacilityTypes,
			HealthcareFacilityTypeList.HealthcareFacilityTypes.Select(c => 
			new TextValue(
				$"{c.System}|{c.Code}|{c.Display}",	// Value in select list
				$"{c.Display} [{c.Code}]"			// What the user sees
			)),
			nameof(TextValue.Value),				// Name of property used for value
			nameof(TextValue.Text)                  // Name of property used for text
		);

		/*HealthcareFacilityTypeList.HealthcareFacilityTypes.Select(c => new
		{
			Value = $"{c.System}|{c.Code}|{c.Display}",  // matches your earlier option values
			Text = $"{c.Display} [{c.Code}]"             // what the user sees
		}),*/

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
	public async Task<IActionResult> Classification(ClassificationViewModel model, NavigationAction navButton)
	{
		SaveMetadataToSession(model.Metadata); 
		
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

	public async Task<IActionResult> FinalizeSubmission()
	{
		var model = new FinalizeSubmissionViewModel();

		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest;

		model.Upload = request;

		model.JsonBundleDocument = _bundleCreator.CreateFhirBundle(request, "Comprehensive");

		return View("Steps/FinalizeSubmission", model);
	}

	public IActionResult ViewJsonFhirBundle()
	{	
		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest;		

		var bundleDocument = _bundleCreator.CreateFhirBundle(request, "Comprehensive");

		return Content(bundleDocument, "application/json"); 
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

		await _phrApiService.ProvideDocumentBundleAsync(sessionData.AccessToken, Constants.JwkPrivateKey, bundleDocument);
		//await _phrApiService.CreateDocumentReferenceAsync(bundleDocument); 

		//await RegisterBundleAsync("test"); 
		

		if (navButton == NavigationAction.Next)
		{
			return RedirectToAction(nameof(SubmissionConfirmation));
		}

		return View();
	}

	/*public async Task RegisterBundleAsync(string documentBundle)
	{
		try
		{
			var result = await ApiService.CreateDocumentReferanceAsync(documentBundle);

			//var state = PatientContextHandler.UpsertPatientSessionData(userSessionData.Identities.FirstOrDefault()!.Name!, patient);

			if (result != null!)
			{
				ShowMessage(ToastType.Info, "");
				//statusTask = "Completed";
			}
			;

			message = result.ToString(); // await result.Content.ReadAsStringAsync();
			StateHasChanged();

		}
		catch (AccessTokenNotAvailableException exception)
		{
			exception.Redirect();
		}
		catch (Exception ex)
		{
			throw ex;
		}
	} */

	public async Task<IActionResult> SubmissionConfirmation()
	{
		var model = new FinalizeSubmissionViewModel();

		var sessionData = session.SessionData();
		var request = sessionData.BundleRequest;

		model.Upload = request;

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
