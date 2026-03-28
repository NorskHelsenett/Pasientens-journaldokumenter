using NorskHelsenettUpload;
using NorskHelsenettUpload.FluidExtensions;
using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Models;
using PatientHealthRecordsEPJ.UploadDocuments.Lists;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Custom.RegistryDtos.TestData;
using XcaXds.Commons.Serializers;
using XcaXds.Commons.Services;

namespace PatientHealthRecordsEPJ.Services;

public interface ICreateDocumentService
{
	Task<ProvideDocumentBundleRequest> GenerateRandomTestDocuments(ProvideDocumentBundleRequest request, SessionData sessionData, int entriesToGenerate, Action<DocumentEntryDto, SubmissionSetDto, AssociationDto>? configureDocument = null);
	Task<Document> CreateDocument(ProvideDocumentBundleRequest request, SessionData sessionData, string fileName, string contentType, byte[] content, DateTime lastModified, bool createRandom = false); 
}; 

public class CreateDocumentService : ICreateDocumentService
{
	private readonly OrganizationService _organizationService;

	public CreateDocumentService(
		OrganizationService organizationService
		)
	{
		_organizationService = organizationService;
	}

	public async Task<ProvideDocumentBundleRequest> GenerateRandomTestDocuments
		(ProvideDocumentBundleRequest request, 
		SessionData sessionData, 
		int entriesToGenerate,
		Action<DocumentEntryDto, SubmissionSetDto, AssociationDto>? configureDocument = null
		)
	{
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

		var generatedTestRegistryObjects = TestDataGeneratorService.GenerateRegistryObjectsFromTestData(jsonTestData, entriesToGenerate, creationTime, configureDocument: configureDocument);

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
					request, 
					sessionData: sessionData,
					//generatedTestObject.Title,
					//generatedTestObject.MimeType!,
					fileName,
					contentType,
					cdaXmlBytes,
					generatedTestObject.CreationTime!.Value,
					createRandom: true
					);

				request.DocumentCollection.WithDocument(document, selectDocument: true);
			}

			/*var cdaDoc = CdaBuilder.FromDocumentEntryDto(generatedTestObject);
			var outputPath = Path.Combine(AppContext.BaseDirectory, "GeneratedCda", $"CDA_{generatedTestObject.Id}.xml");

			Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
			cdaDoc.Save(outputPath); */


		}

		//_registryWrapper.UpdateDocumentRegistryContentWithDtos(generatedTestRegistryObjects);

		return request; 
	}

	private static string ToTitleCase(string s)
	{
		if (string.IsNullOrEmpty(s))
			return s;

		if (char.IsUpper(s[0]))
			return s;

		return char.ToUpperInvariant(s[0]) + s.Substring(1);
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

	public async Task<Document> CreateDocument(ProvideDocumentBundleRequest request, SessionData sessionData, string fileName, string contentType, byte[] content, DateTime lastModified, bool createRandom = false)
	{
		//var sessionData = session.SessionData();
		//var request = sessionData.BundleRequest;

		var extension = Path.GetExtension(fileName);

		string mimeType = NormalizeMime(contentType, extension);

		var formatCode = CompositionFormatDetector.DetermineFormat(content, ref mimeType, fileName);

		Document document = new Document()
			//Document document = upload.DocumentCollection.SelectedDocument		
			.WithUniqueId(Guid.NewGuid())
			.WithFilename(fileName)
			//.WithContent(content, computeSha: true)
			.WithContentSaveToDisk(content, computeSha: true) // save to disk to avoid large memory consumption (and large saves to session/Redis/Valkey) with large files
			.WithMimeType(mimeType)
			.WithCreationTime(lastModified)
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
						else
						{
							// If we can't determine classCode from typeCode, we could try to find it from templateId or other nodes. For now we just leave it empty. 
							document.Metadata.WithClassCode(ClassCodeList.ClassCodes.First());
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

				//document.WithTitle(ToPascalCase(fileNameNoExtension));
				document.WithTitle(ToTitleCase(fileNameNoExtension));

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
}