using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using NorskHelsenettUpload;
using NorskHelsenettUpload.FluidExtensions;
using NorskHelsenettUpload.Ihe.Mhd.Interfaces;
using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Models;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.Models;
using PatientHealthRecordsEPJ.Models.DokumentlagerTest;
using PatientHealthRecordsEPJ.Models.HelsenorgeLogin;
using PatientHealthRecordsEPJ.Services;
using PdfSharpCore;
using System.Reflection.Metadata;
using System.Text.Json;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.ClinicalDocument;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using static NorskHelsenettUpload.Constants.CodeSystems.Hl7.ConfidentialityCode;
using static NorskHelsenettUpload.Constants.CodeSystems.Hl7.PurposeOfUse;
using static NorskHelsenettUpload.Constants.CodeSystems.Volven.ConfidentialityCode;

namespace PatientHealthRecordsEPJ.DokumentlagerTest;

public interface IDokumentlagerTestRunner
{
	System.Threading.Tasks.Task RunTestAsync(string runId, CancellationToken cancellationToken);
}

public sealed class DokumentlagerTestRunner : IDokumentlagerTestRunner
{
	private readonly ITestRunStore _store;
	private readonly ILogger<DokumentlagerTestRunner> _logger;

	private readonly IBundleCreator _bundleCreator;
	private readonly OrganizationService _organizationService;
	private readonly ICreateDocumentService _createDocumentService;
	private readonly ProvideDocumentBundleApiService _provideDocumentsBundleApiService;

	private readonly IPhrApiEnvironmentProvider _phrApiEnvironmentProvider;

	private readonly IHttpClientFactory _httpClientFactory;

	private readonly IOidcClientService _oidcClientService;

	public DokumentlagerTestRunner(
		ITestRunStore store,
		ILogger<DokumentlagerTestRunner> logger,
		IBundleCreator bundleCreator,
		OrganizationService organizationService,
		ICreateDocumentService createDocumentService,
		ProvideDocumentBundleApiService provideDocumentsBundleApiService,
		IPhrApiEnvironmentProvider phrApiEnvironmentProvider,
		IHttpClientFactory httpClientFactory,
		IOidcClientService oidcClientService
		)
	{
		_store = store;
		_logger = logger;
		_bundleCreator = bundleCreator;
		_organizationService = organizationService;
		_createDocumentService = createDocumentService;
		_provideDocumentsBundleApiService = provideDocumentsBundleApiService;
		_phrApiEnvironmentProvider = phrApiEnvironmentProvider;
		_httpClientFactory = httpClientFactory;
	}

	/*public async Task<IActionResult> RefreshToken()
	{
		var redirectUrl = GetRedirectUrl();

		await _oidcClientService.InitializeAsync(redirectUrl);

		var loginResult = session.SessionData();

		var parameters = new Duende.IdentityModel.Client.Parameters
			{
				{"resource", HomeController.PhrResource}
			};

		var result = await _oidcClientService.RefreshTokenAsync(loginResult.RefreshToken, parameters);

		if (result.IsError)
		{
			throw new Exception($"{result.Error}: {result.ErrorDescription}");
		}

		loginResult.AccessToken = result.AccessToken;
		loginResult.RefreshToken = result.RefreshToken;
		loginResult.SaveToSession(session);

		return Ok(result.AccessToken);
	} */

	public async System.Threading.Tasks.Task RunTestAsync(string runId, CancellationToken cancellationToken)
	{
		if (!_store.TryGet(runId, out var run) || run is null)
		{
			_logger.LogWarning("RunId {RunId} not found.", runId);
			return;
		}

		try
		{
			run.Status = DokumentlagerTestCaseStatus.Running;
			_store.Update(run);

			// Simulate long-running work
			//await Task.Delay(3000, cancellationToken);

			// Put your real test logic here
			//var result = await DoActualTestAsync(run.GroupIndex, run.TestIndex, run.PatientId, cancellationToken);
			var result = await DoActualTestAsync(run, cancellationToken);

			run.Result = result;
			//run.Status = DokumentlagerTestCaseStatus.Success;

			/*run.Status = Random.Shared.Next(0, 2) == 0
				? DokumentlagerTestCaseStatus.Success
				: DokumentlagerTestCaseStatus.Failed;*/

			_store.Update(run);
		}
		catch (OperationCanceledException)
		{
			run.Status = DokumentlagerTestCaseStatus.Failed;
			run.Error = "Cancelled";
			_store.Update(run);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Test run failed for {RunId}", runId);

			run.Status = DokumentlagerTestCaseStatus.Failed;
			run.Error = ex.Message;
			_store.Update(run);
		}
	}

	private async Task<DokumentlagerTestResult> DoActualTestAsync(
		TestRunInfo run,
		//int groupIndex,
		//int testIndex,
		//string patientId,
		CancellationToken cancellationToken)
	{
		/*await System.Threading.Tasks.Task.Delay(2000, cancellationToken);

		return new DokumentlagerTestResult
		{
			Success = true,
			Message = $"Finished test {run.GroupIndex}/{run.TestIndex} for patient {run.PatientId}"
		};*/

		var test = run.Test;
		var sessionData = run.SessionData;

		//test.Status = Random.Shared.Next(0, 2) == 0
		//	? DokumentlagerTestCaseStatus.Success
		//	: DokumentlagerTestCaseStatus.Failed;

		var lookup = test.Lookup;

		var lookupDokumentlager = lookup.LookupDokumentlager;
		var lookupAPI = lookup.LookupAPI;
		var lookupKJ = lookup.LookupKJ;
		var lookupHelsenorge = lookup.LookupHelsenorge;

		/*lookupDokumentlager.Success = Random.Shared.Next(0, 2) == 0;
		lookupAPI.Success = Random.Shared.Next(0, 2) == 0;
		lookupKJ.Success = Random.Shared.Next(0, 2) == 0;
		lookupHelsenorge.Success = Random.Shared.Next(0, 2) == 0; */

		test.Status = DokumentlagerTestCaseStatus.Running;

		_store.Update(run);

		var homeCommunityId = "2.16.578.1.12.4.5.100.3.15";

		DokumentlagerTestResult result = new DokumentlagerTestResult()
		{
			Success = false
		};

		// Create new submission and documents

		var bundleRequest = new ProvideDocumentBundleRequest();

		var submissionTitle = $"SubmissionSet-{run.PatientId}-{DateTime.Now.ToString()}";

		bundleRequest.Submission
			.WithUniqueId(Guid.NewGuid())
			.WithAvailabilityStatus(DocumentSubmissionMetadata.AvailabilityStatusApproved)
			.WithSubmissionTime(DateTime.Now)
			.WithSourceId("1.1") // TODO: what to use as sourceId? For now just a hardcoded dummy. 
			.WithPatientId(run.PatientId)
			.WithTitle(submissionTitle);

		bundleRequest.Submission.WithHomeCommunityId(homeCommunityId);

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
		var author = new NorskHelsenettUpload.Ihe.Mhd.Models.Author
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

		bundleRequest.Patient = sessionData
				.SourcePatientInfo!
				.WithXdsGlobalIdentifier("1.2.3") // If we fix/change this, also fix in PatientController
				.WithXdsHomeCommunity("1.2.3")
				;

		void ConfigureDocument(DocumentEntryDto documentEntry, SubmissionSetDto submissionSet, AssociationDto association)
		{
			if (test.SecurityLabels.Count > 0)
			{
				documentEntry.ConfidentialityCode = new List<CodedValue>();

				foreach (var securityLabel in test.SecurityLabels)
				{
					var system = string.Empty;

					if (securityLabel == Normal || securityLabel == Restricted || securityLabel == VeryRestricted || securityLabel == Low || securityLabel == Unrestricted)
					{
						system = NorskHelsenettUpload.Constants.CodeSystems.Hl7.ConfidentialityCode.Oid;
					}
					else
					{
						system = NorskHelsenettUpload.Constants.CodeSystems.Volven.ConfidentialityCode.Oid;
					}

					documentEntry.ConfidentialityCode.Add(
						new CodedValue
						{
							Code = securityLabel,
							CodeSystem = system
						}
					);
				}
			}
		}

		bundleRequest = await _createDocumentService.GenerateRandomTestDocuments(bundleRequest, sessionData, 1, ConfigureDocument);

		var firstDocument = bundleRequest.DocumentCollection.Documents.FirstOrDefault();

		var documentReferenceId = firstDocument?.Metadata.UniqueId; // CreateFhirBundle uses UniqueId as DocumentReference id, so we can use it here to identify the uploaded document in the lookup step later.

		// Upload document

		var bundleDocument = _bundleCreator.CreateFhirBundle(bundleRequest, "Comprehensive");

		var validate = false; 

		var apiResult = await _provideDocumentsBundleApiService.ProvideBundle.ProvideDocumentBundleAsync(bundleDocument, validate);

		test.Status = DokumentlagerTestCaseStatus.Success;
		test.Result.Success = true;

		_store.Update(run);

		//sessionData.SaveToSession(session);

		if (apiResult.IsSuccess)
		{
			var responseBody = apiResult.Body;
			result.Success = true;
		}
		else
		{
			result.Success = false;
			/*model.ErrorMessage = "An error occurred trying to register document bundle";
			model.IsError = true;
			model.ResponseBody = apiResult.Body;
			model.Upload = request;

			model.JsonBundleDocument = _bundleCreator.CreateFhirBundle(request, "Comprehensive", removeBinaryDataForDisplay: true);
			model.JsonBundleDocument = model.JsonBundleDocument.Replace(BundleCreator.REMOVED_FOR_DISPLAY_TEXT_BASE64, "...removed for display...");

			return View("Steps/FinalizeSubmission", model);*/
		}


		//
		// Get document list
		//

		// From Dokumentlager

		var httpClient = _httpClientFactory.CreateClient();

		if (lookupDokumentlager != null)
		{
			if (lookupDokumentlager.IsTryRetrieveDocumentReference || lookupDokumentlager.IsTryOpenDocument)
			{
				if (lookupDokumentlager.IsTryRetrieveDocumentReference)
				{
					lookupDokumentlager.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.Running;
					_store.Update(run);
				}

				if (lookupDokumentlager.IsTryOpenDocument)
				{
					lookupDokumentlager.OpenDocumentStatus = LookupDocumentStatus.Running;
					_store.Update(run);
				}

				var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

				var getDocumentStatusApiUrl = $"{_apiEnvironment.RootUrl}/registry-repository/document-status?home={homeCommunityId}&document={documentReferenceId}";

				var getDocumentStatusResponse = await httpClient.GetAsync(getDocumentStatusApiUrl);

				if (getDocumentStatusResponse.IsSuccessStatusCode)
				{
					var responseContent = await getDocumentStatusResponse.Content.ReadAsStringAsync();

					var documentStatusResult = JsonSerializer.Deserialize<DocumentStatusResult>(responseContent);

					if (lookupDokumentlager.IsTryRetrieveDocumentReference)
					{
						lookupDokumentlager.RetrieveDocumentReferenceIsSuccess = documentStatusResult.document.savedToRegistry;
						lookupDokumentlager.RetrieveDocumentReferenceStatus = lookupDokumentlager.RetrieveDocumentReferenceIsSuccess ? LookupDocumentReferenceStatus.Visible : LookupDocumentReferenceStatus.NotVisible;
					}

					if (lookupDokumentlager.IsTryOpenDocument)
					{
						lookupDokumentlager.OpenDocumentIsSuccess = documentStatusResult.document.savedToRepository;
						lookupDokumentlager.OpenDocumentStatus = lookupDokumentlager.OpenDocumentIsSuccess ? LookupDocumentStatus.OpenDocumentSuccess : LookupDocumentStatus.OpenDocumentFailed;
					}
				}
				else
				{
					lookupDokumentlager.RetrieveDocumentReferenceIsSuccess = false;
					lookupDokumentlager.IsTryOpenDocument = false;
				}

				_store.Update(run);
			}
		}

		// From REST API

		if (lookupAPI != null)
		{
			Uri? retrieveDocumentUri = null;

			if (lookupAPI.IsTryRetrieveDocumentReference)
			{

				lookupAPI.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.Running;
				_store.Update(run);

				var headerKeyValues = new List<KeyValuePair<string, string>>();

				var grunnlagId = test.GrunnlagId ?? "SAMTYKKE";

				//if (sessionLoginResult.DocumentListScopeId == (int)DocumentListScopeEnum.Kjernejournalforskriften)
				{
					// Only send access basis if using scope 'nhn:phr/mhd/read-documentreferences/kjernejournalforskriften'
					headerKeyValues.Add(new KeyValuePair<string, string>("nhn-access-basis", grunnlagId ?? string.Empty));
				}

				headerKeyValues.Add(new KeyValuePair<string, string>("nhn-source-system", "PJD Test EPJ"));
				headerKeyValues.Add(new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()));

				//headerKeyValues.Add(new KeyValuePair<string, string>("nhn-bypass-cache", sessionLoginResult.UseCaching ? "false" : "true"));
				headerKeyValues.Add(new KeyValuePair<string, string>("nhn-bypass-cache", "true"));

				var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

				var apiUrl = _apiEnvironment.GetDocumentReferencesApiUrl;

				var urlencodedContent = new FormUrlEncodedContent([
					//new("patient.identifier", sessionLoginResult.SourcePatientInfo?.NIN),
					new("patient.identifier", run.PatientId),
					new("status", "current"),
					//new("_count", pageSize.ToString()),
					//new("_page", pageIndex.ToString()),
					//new("_sort", sort),
					// If _include contains DocumentReference:relatesTo, get associations. Note: Will make an extra call to XDS for associations and slow down UI rendering.
					// OK for Test EPJ so that we can show associations in the UI in the "Info" column. 
					// In production, consider calling a separate API (GetDocumentAssociations) to get associations only when user requests it for a specific document.			
					//new("_include", "DocumentReference:relatesTo")
				]);

				var apiGetDocumentListResult = await ApiCaller.CallApi(HttpMethod.Post,
					apiUrl,
					sessionData.PjdRepositoryAccessToken!,
					document: false,
					headerKeyValues: headerKeyValues,
					acceptHeaderValues: ["application/fhir+json; fhirVersion=4.0"], // Only empty accept header, or 'application/fhir+json' is allowed				
					content: urlencodedContent
					);

				if (apiGetDocumentListResult.IsSuccess)
				{
					var responseBody = apiGetDocumentListResult.Body;

					var serializerOptions = new JsonSerializerOptions().ForFhir(ModelInfo.ModelInspector).Pretty();
					var bundle = JsonSerializer.Deserialize<Bundle>(responseBody, serializerOptions);

					/*var documentReferences = bundle?.Entry?
						.Where(e => e.Resource?.ResourceType == ResourceType.DocumentReference)
						.Select(e => e.Resource as DocumentReference)
						.ToList();*/

					var documentReferences = bundle?.Entry
						.Select(e => e.Resource)
						.OfType<DocumentReference>()
						.ToList();

					var uploadedDocumentReference = documentReferences?.FirstOrDefault(dr => dr.Id == documentReferenceId.ToString());

					if (uploadedDocumentReference != null)
					{
						var title = "****";
						if (title.Contains("**"))
						{
							lookupAPI.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.VisibleWithObfuscation;
						}
						else
						{

							string? GetHomeCommunityId(DocumentReference document)
							{
								var homeCommunityIdExtension = document.Extension.FirstOrDefault(x => x.Url == "http://ihe.net/fhir/StructureDefinition/ihe-homeCommunityId");

								var homeCommunityId = homeCommunityIdExtension?.Value.ToString();

								return homeCommunityId;
							}

							string? GetRepositoryUniqueId(DocumentReference document)
							{
								var contentComponent = document.Content.FirstOrDefault();

								if (contentComponent == null)
									return string.Empty;

								var attachment = contentComponent.Attachment;

								var repositoryUniqueIdExtension = attachment.Extension.FirstOrDefault(x => x.Url == "http://ihe.net/fhir/StructureDefinition/ihe-repositoryUniqueId");

								var repositoryUniqueId = repositoryUniqueIdExtension?.Value.ToString();

								return repositoryUniqueId;
							}

							lookupAPI.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.Visible;

							var contentComponent = uploadedDocumentReference.Content.FirstOrDefault();
							var attachment = contentComponent.Attachment;
							var format = contentComponent.Format;

							var securityLabels = uploadedDocumentReference.SecurityLabel;
							//var documentReferenceId = document.Id;
							var documentUniqueId = uploadedDocumentReference.MasterIdentifier.Value;
							//var homeCommunityId = GetHomeCommunityId(uploadedDocumentReference);
							var repositoryUniqueId = GetRepositoryUniqueId(uploadedDocumentReference);

							retrieveDocumentUri = new Uri(attachment.Url);

							//var deleteUrl = $"documents/DeleteDocument?id={documentReferenceId}&homeCommunityId={homeCommunityId}"; // Kaller DocumentsController->Delete metoden for å slette dokument

						}
					}
					else
					{
						lookupAPI.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.NotVisible;
					}

					if (lookupAPI.RetrieveDocumentReferenceExpectVisible)
					{
						// Success if the document reference was retrieved and fully visible
						lookupAPI.RetrieveDocumentReferenceIsSuccess = lookupAPI.RetrieveDocumentReferenceStatus == LookupDocumentReferenceStatus.Visible;
					}
					else if (lookupAPI.RetrieveDocumentReferenceExpectVisibleWithObfuscation)
					{
						// Success if the document reference was retrieved and visible with obfuscation
						lookupAPI.RetrieveDocumentReferenceIsSuccess = lookupAPI.RetrieveDocumentReferenceStatus == LookupDocumentReferenceStatus.VisibleWithObfuscation;
					}
					else if (!lookupAPI.RetrieveDocumentReferenceExpectVisible)
					{
						lookupAPI.RetrieveDocumentReferenceIsSuccess = lookupAPI.RetrieveDocumentReferenceStatus == LookupDocumentReferenceStatus.NotVisible;
					}

					/*if (lookupAPI.RetrieveDocumentReferenceIsSuccess || lookupAPI.RetrieveDocumentReferenceIsSuccessWithObfuscation)
					{
						lookupAPI.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.Success;						
					}
					else
					{
						lookupAPI.RetrieveDocumentReferenceStatus = LookupStatus.Failed;
					}*/
				}
				else
				{
					lookupAPI.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.NotVisible;
					lookupAPI.RetrieveDocumentReferenceIsSuccess = false;
				}

				_store.Update(run);
			}

			if (lookupAPI.IsTryOpenDocument)
			{
				//lookupAPI.OpenDocumentIsSuccess = true;
				//lookupAPI.OpenDocumentIsSuccess = Random.Shared.Next(0, 2) == 0; // Simulate open document success or failure randomly for now, since we don't have a real document viewer implemented in Test EPJ.
				//lookupAPI.OpenDocumentStatus = lookupAPI.OpenDocumentIsSuccess  ? LookupDocumentStatus.OpenDocumentSuccess : LookupDocumentStatus.OpenDocumentFailed;

				// Get document from REST API

				lookupAPI.OpenDocumentStatus = LookupDocumentStatus.Running;
				_store.Update(run);

				if (retrieveDocumentUri == null)
				{
					lookupAPI.OpenDocumentIsSuccess = false;
					lookupAPI.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentFailed;
				}
				else
				{
					var query = QueryHelpers.ParseQuery(retrieveDocumentUri.Query);
					var documentUniqueId = query["documentUniqueId"];
					//var tempDocs = Path.Combine(Path.GetTempPath(), "documents");   // In Program.cs there's a UseStaticFiles which serves from this path
					//var patientFolder = Path.Combine(tempDocs, sessionData.PatientId);
					//var patientRelativePath = $"/documents/{sessionData.PatientId}";
					//Directory.CreateDirectory(patientFolder);    // For storing documents which will be displayed

					GetDocumentViewModel model = new GetDocumentViewModel();

					var headerKeyValues = new List<KeyValuePair<string, string>>();

					if (test.DocumentListScope == DocumentListScopeEnum.Kjernejournalforskriften)
					{
						// Only send access basis if using scope 'nhn:phr/mhd/read-documentreferences/kjernejournalforskriften', and only if FORHOYET_SAMTYKKE or FORHOYET_AKUTT

						if (test.GrunnlagId?.Contains("forhoyet") == true)
						{
							headerKeyValues.Add(new KeyValuePair<string, string>("nhn-access-basis", test.GrunnlagId ?? string.Empty));
						}
					}

					// We send the patient Id in the header as nhn-patient-nin to avoid having the patient Id in the URL.             

					headerKeyValues.Add(new KeyValuePair<string, string>("nhn-patient-nin", sessionData.PatientId));
					headerKeyValues.Add(new KeyValuePair<string, string>("nhn-source-system", "PHR Test EPJ integration tests"));
					headerKeyValues.Add(new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()));

					var apiGetDocumentResult = await ApiCaller.CallApi(HttpMethod.Get, retrieveDocumentUri.ToString(), sessionData.PjdRepositoryAccessToken!, document: true, headerKeyValues: headerKeyValues);

					if (apiGetDocumentResult.IsSuccess)
					{
						lookupAPI.OpenDocumentIsSuccess = true;
						lookupAPI.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentSuccess;
					}
					else
					{
						lookupAPI.OpenDocumentIsSuccess = false;
						lookupAPI.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentFailed;
					}

					_store.Update(run);
				}
			}

			if (lookupAPI.IsTryReadAuditEvents)
			{

			}

			/*var apiGetResult = await _provideDocumentsBundleApiService.GetDocumentsForPatientAsync(run.PatientId);
			if (apiGetResult.IsSuccess)
			{
				var responseBody = apiGetResult.Body;
				lookupAPI.Success = true;
			}
			else
			{
				lookupAPI.Success = false;
			}*/

			_store.Update(run);
		}


		// From Kjernejournal

		if (lookupKJ != null)
		{
			KjernejournalDocumentList kjernejournalDocumentList = KjernejournalDocumentList.CreateRandomDocumentList(5); // Simulate retrieving document list from Kjernejournal. 

			Uri? kjRetrieveDocumentUri = null;

			if (lookupKJ.IsTryRetrieveDocumentReference)
			{
				lookupKJ.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.Running;
				_store.Update(run);

				await System.Threading.Tasks.Task.Delay(2000, cancellationToken); // Simulate time taken to retrieve document list from Kjernejournal

				var kjDocumentReference = kjernejournalDocumentList.journalDocuments?.FirstOrDefault(dr => dr.documentUniqueId == documentReferenceId.ToString());

				if (kjDocumentReference != null)
				{
					var title = kjDocumentReference.documentName;
					if (title.Contains("**"))
					{
						lookupKJ.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.VisibleWithObfuscation;
					}
					else
					{
						lookupKJ.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.Visible;

						//var documentReferenceId = document.Id;
						var documentUniqueId = kjDocumentReference.documentUniqueId;
						//var homeCommunityId = GetHomeCommunityId(uploadedDocumentReference);
						var repositoryUniqueId = kjDocumentReference.repositoryUniqueId;

						//kjRetrieveDocumentUri = new Uri(attachment.Url);

						//var deleteUrl = $"documents/DeleteDocument?id={documentReferenceId}&homeCommunityId={homeCommunityId}"; // Kaller DocumentsController->Delete metoden for å slette dokument

					}
				}
				else
				{
					lookupKJ.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.NotVisible;
				}

				if (lookupKJ.RetrieveDocumentReferenceExpectVisible)
				{
					// Success if the document reference was retrieved and fully visible
					lookupKJ.RetrieveDocumentReferenceIsSuccess = lookupKJ.RetrieveDocumentReferenceStatus == LookupDocumentReferenceStatus.Visible;
				}
				else if (lookupKJ.RetrieveDocumentReferenceExpectVisibleWithObfuscation)
				{
					// Success if the document reference was retrieved and visible with obfuscation
					lookupKJ.RetrieveDocumentReferenceIsSuccess = lookupKJ.RetrieveDocumentReferenceStatus == LookupDocumentReferenceStatus.VisibleWithObfuscation;
				}
				else if (!lookupKJ.RetrieveDocumentReferenceExpectVisible)
				{
					lookupKJ.RetrieveDocumentReferenceIsSuccess = lookupKJ.RetrieveDocumentReferenceStatus == LookupDocumentReferenceStatus.NotVisible;
				}
			}
			else
			{
				lookupKJ.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.NotVisible;				
				lookupKJ.RetrieveDocumentReferenceIsSuccess = !lookupKJ.RetrieveDocumentReferenceExpectVisible && lookupKJ.RetrieveDocumentReferenceStatus == LookupDocumentReferenceStatus.NotVisible;
			}

			_store.Update(run);

			if (lookupKJ.IsTryOpenDocument)
			{
				lookupKJ.OpenDocumentStatus = LookupDocumentStatus.Running;
				_store.Update(run);

				await System.Threading.Tasks.Task.Delay(2000, cancellationToken);

				if (kjRetrieveDocumentUri == null)
				{
					lookupKJ.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentFailed;
					lookupKJ.OpenDocumentIsSuccess = !lookupKJ.OpenDocumentExpectSuccess;
				}
				else
				{
					var getKjernejournalDocumentUrl = "https://google.com";

					var getDocumentStatusResponse = await httpClient.GetAsync(getKjernejournalDocumentUrl);

					lookupKJ.OpenDocumentStatus = getDocumentStatusResponse.IsSuccessStatusCode ? LookupDocumentStatus.OpenDocumentSuccess : LookupDocumentStatus.OpenDocumentFailed;

					lookupKJ.OpenDocumentIsSuccess = lookupKJ.OpenDocumentExpectSuccess && lookupKJ.OpenDocumentStatus == LookupDocumentStatus.OpenDocumentSuccess
						|| (!lookupKJ.OpenDocumentExpectSuccess && lookupKJ.OpenDocumentStatus == LookupDocumentStatus.OpenDocumentFailed);
				}

				_store.Update(run);
			}
		}

		// From Helsenorge
		if (lookupHelsenorge != null)
		{
			HelsenorgeDocumentList helsenorgeDocumentList = HelsenorgeDocumentList.CreateRandomDocumentList(5); // Simulate retrieving document list from Helsenorge. 

			Uri? hnRetrieveDocumentUri = null;

			if (lookupHelsenorge.IsTryRetrieveDocumentReference)
			{
				lookupHelsenorge.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.Running;
				_store.Update(run);

				await System.Threading.Tasks.Task.Delay(2000, cancellationToken); // Simulate time taken to retrieve document list from Kjernejournal

				var hnDocumentReference = helsenorgeDocumentList.Dokumenter?.FirstOrDefault(dr => dr.Id == documentReferenceId.ToString());

				if (hnDocumentReference != null)
				{
					var title = hnDocumentReference.Tittel;
					if (title.Contains("**"))
					{
						lookupHelsenorge.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.VisibleWithObfuscation;
					}
					else
					{
						lookupHelsenorge.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.Visible;

						//var documentReferenceId = document.Id;
						var documentUniqueId = hnDocumentReference.Id;
						//var homeCommunityId = hnDocumentReference.HomeId;
						var repositoryUniqueId = hnDocumentReference.RepoId;

						//kjRetrieveDocumentUri = new Uri(attachment.Url);

						//var deleteUrl = $"documents/DeleteDocument?id={documentReferenceId}&homeCommunityId={homeCommunityId}"; // Kaller DocumentsController->Delete metoden for å slette dokument

					}
				}
				else
				{
					lookupHelsenorge.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.NotVisible;
				}

				if (lookupHelsenorge.RetrieveDocumentReferenceExpectVisible)
				{
					// Success if the document reference was retrieved and fully visible
					lookupHelsenorge.RetrieveDocumentReferenceIsSuccess = lookupHelsenorge.RetrieveDocumentReferenceStatus == LookupDocumentReferenceStatus.Visible;
				}
				else if (lookupHelsenorge.RetrieveDocumentReferenceExpectVisibleWithObfuscation)
				{
					// Success if the document reference was retrieved and visible with obfuscation
					lookupHelsenorge.RetrieveDocumentReferenceIsSuccess = lookupHelsenorge.RetrieveDocumentReferenceStatus == LookupDocumentReferenceStatus.VisibleWithObfuscation;
				}
				else if (!lookupHelsenorge.RetrieveDocumentReferenceExpectVisible)
				{
					lookupHelsenorge.RetrieveDocumentReferenceIsSuccess = lookupHelsenorge.RetrieveDocumentReferenceStatus == LookupDocumentReferenceStatus.NotVisible;
				}
			}
			else
			{
				lookupHelsenorge.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.NotVisible;
				lookupHelsenorge.RetrieveDocumentReferenceIsSuccess = !lookupHelsenorge.RetrieveDocumentReferenceExpectVisible && lookupHelsenorge.RetrieveDocumentReferenceStatus == LookupDocumentReferenceStatus.NotVisible;
			}

			_store.Update(run);

			if (lookupHelsenorge.IsTryOpenDocument)
			{
				lookupHelsenorge.OpenDocumentStatus = LookupDocumentStatus.Running;
				_store.Update(run);

				await System.Threading.Tasks.Task.Delay(2000, cancellationToken);

				if (hnRetrieveDocumentUri == null)
				{
					lookupHelsenorge.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentFailed;
					lookupHelsenorge.OpenDocumentIsSuccess = !lookupHelsenorge.OpenDocumentExpectSuccess;
				}
				else
				{
					var getHelsenorgeDocumentUrl = "https://google.com";

					var getDocumentStatusResponse = await httpClient.GetAsync(getHelsenorgeDocumentUrl);

					lookupHelsenorge.OpenDocumentStatus = getDocumentStatusResponse.IsSuccessStatusCode ? LookupDocumentStatus.OpenDocumentSuccess : LookupDocumentStatus.OpenDocumentFailed;

					lookupHelsenorge.OpenDocumentIsSuccess = lookupHelsenorge.OpenDocumentExpectSuccess && lookupHelsenorge.OpenDocumentStatus == LookupDocumentStatus.OpenDocumentSuccess
						|| (!lookupHelsenorge.OpenDocumentExpectSuccess && lookupHelsenorge.OpenDocumentStatus == LookupDocumentStatus.OpenDocumentFailed);
				}

				_store.Update(run);
			}
		}	

		if (test.IsTryPatchDocument)
		{
			test.PatchDocumentStatus = DokumentlagerTestCaseStatus.Running;
			_store.Update(run);

			//await System.Threading.Tasks.Task.Delay(2000, cancellationToken); // Simulate time taken to patch document); 

			var body = """
			   {
			  "resourceType": "DocumentReference",
			  "id": "_DOCUMENTREFERENCE_ID_",
			  "securityLabel": [
			     {
			            "coding": [
			                {
			                    "system": "urn:oid:2.16.840.1.113883.5.25",
			                    "code": "N",
			                    "display": "Normal"
			                }
			            ]
			        },
			        {
			            "coding": [
			                {
			                    "system": "urn:oid:2.16.578.1.12.4.1.1.9603",
			                    "code": "NORS",
			                    "display": "Sperret"
			                }
			            ]
			        }
			  ]
			}
			
			""";

			body = body.Replace("_DOCUMENTREFERENCE_ID_", documentReferenceId.ToString());
			
			var headerKeyValues = new List<KeyValuePair<string, string>>();

			headerKeyValues.Add(new KeyValuePair<string, string>("nhn-source-system", "PHR Test EPJ integration tests"));
			headerKeyValues.Add(new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()));

			var content = new StringContent(body, null, "application/fhir+json");
			
			var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

			var apiUrl = $"{_apiEnvironment.PatchDocumentUrl}/{documentReferenceId}?homeCommunityId={homeCommunityId}";

			var apiPatchDocumentResult = await ApiCaller.CallApi(HttpMethod.Patch,
				apiUrl,
				sessionData.PjdRepositoryAccessToken!,
				document: false,
				headerKeyValues: headerKeyValues,
				content: content
				);

			if (apiPatchDocumentResult.IsSuccess)
			{
				test.PatchDocumentStatus = DokumentlagerTestCaseStatus.Success;
			}
			else
			{
				test.PatchDocumentStatus = DokumentlagerTestCaseStatus.Failed;
			}

			_store.Update(run);
		}

		if (test.IsTryDeleteDocument)
		{
			test.DeleteDocumentStatus = DokumentlagerTestCaseStatus.Running;
			_store.Update(run);			

			//homeCommunityId = homeCommunityId.NoUrn();

			var headerKeyValues = new List<KeyValuePair<string, string>>();

			headerKeyValues.Add(new KeyValuePair<string, string>("nhn-source-system", "PHR Test EPJ integration tests"));
			headerKeyValues.Add(new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()));

			var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

			var apiUrl = $"{_apiEnvironment.DeleteDocumentUrl}/{documentReferenceId}?homeCommunityId={homeCommunityId}";

			var apiDeleteDocumentResult = await ApiCaller.CallApi(HttpMethod.Delete,
				apiUrl,
				sessionData.PjdRepositoryAccessToken!,
				document: false,
				headerKeyValues: headerKeyValues
				);

			if (apiDeleteDocumentResult.IsSuccess)
			{
				test.DeleteDocumentStatus = DokumentlagerTestCaseStatus.Success;
			}
			else
			{
				test.DeleteDocumentStatus = DokumentlagerTestCaseStatus.Failed;
			}

			_store.Update(run);
		}

		//test.Lookup.LookupAPI.OpenDocumentStatus = LookupDocumentStatus.Running;

		_store.Update(run);

		//await System.Threading.Tasks.Task.Delay(2000, cancellationToken); // Simulate time taken to open document	

		//test.Lookup.LookupAPI.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentSuccess;
		//test.Lookup.LookupAPI.OpenDocumentIsSuccess = true;

		_store.Update(run);

		//await System.Threading.Tasks.Task.Delay(2000, cancellationToken); // Simulate time taken to open document	


		System.Diagnostics.Debug.WriteLine($"Test '{test.Name}' completed with result: {(result.Success ? "Success" : "Failure")}");

		// Calculate success

		result.Success = (test.Lookup?.LookupDokumentlager?.Success == true || test.Lookup?.LookupDokumentlager == null)
			&& (test.Lookup?.LookupAPI?.Success == true || test.Lookup?.LookupAPI == null)
			&& (test.Lookup?.LookupKJ?.Success == true || test.Lookup?.LookupKJ == null)
			&& (test.Lookup?.LookupHelsenorge?.Success == true || test.Lookup?.LookupHelsenorge == null);

		test.Result = result;

		run.Status = result.Success ? DokumentlagerTestCaseStatus.Success : DokumentlagerTestCaseStatus.Failed;

		_store.Update(run);

		//test.Status = result.Success ? DokumentlagerTestCaseStatus.Success : DokumentlagerTestCaseStatus.Failed; 

		//return result;

		return new DokumentlagerTestResult
		{
			Success = true,
			Message = $"Finished test {run.GroupIndex}/{run.TestIndex} for patient {run.PatientId}"
		};
	}
}