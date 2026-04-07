using Duende.IdentityModel.Internal;
using Hl7.Fhir.Model;
using Hl7.Fhir.Model.CdsHooks;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification;
using Microsoft.AspNetCore.WebUtilities;
using MigraDocCore.DocumentObjectModel;
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
using PatientHealthRecordsEPJ.UploadDocuments.Lists;
using PdfSharpCore;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using XcaXds.Commons.Extensions;
using XcaXds.Commons.Models.ClinicalDocument;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using XcaXds.Commons.Models.Soap;
using XcaXds.Commons.Models.Soap.XdsTypes;
using XcaXds.Commons.Serializers;
using static NorskHelsenettUpload.Constants.CodeSystems.Hl7.ConfidentialityCode;
using static NorskHelsenettUpload.Constants.CodeSystems.Hl7.PurposeOfUse;
using static NorskHelsenettUpload.Constants.CodeSystems.Volven.ConfidentialityCode;
using static System.Net.Mime.MediaTypeNames;

namespace PatientHealthRecordsEPJ.DokumentlagerTest;

public interface IDokumentlagerTestRunner
{
	System.Threading.Tasks.Task RunTestAsync(string runId, CancellationToken cancellationToken);
}

public sealed class DokumentlagerTestRunner : IDokumentlagerTestRunner
{
	public const string DokumentlagerTestHttpClientName = "DokumentlagerTestClient"; 

	private readonly ITestRunStore _store;
	private readonly ILogger<DokumentlagerTestRunner> _logger;

	private readonly IBundleCreator _bundleCreator;
	private readonly OrganizationService _organizationService;
	private readonly ICreateDocumentService _createDocumentService;
	private readonly ProvideDocumentBundleApiService _provideDocumentsBundleApiService;

	private readonly IPhrApiEnvironmentProvider _phrApiEnvironmentProvider;

	private readonly IHttpClientFactory _httpClientFactory;

	private readonly IOidcClientService _oidcClientService;

	private readonly IWebHostEnvironment _webHostEnvironment;

	public DokumentlagerTestRunner(
		ITestRunStore store,
		ILogger<DokumentlagerTestRunner> logger,
		IBundleCreator bundleCreator,
		OrganizationService organizationService,
		ICreateDocumentService createDocumentService,
		ProvideDocumentBundleApiService provideDocumentsBundleApiService,
		IPhrApiEnvironmentProvider phrApiEnvironmentProvider,
		IHttpClientFactory httpClientFactory,
		IOidcClientService oidcClientService,
		IWebHostEnvironment webHostEnvironment
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
		_webHostEnvironment = webHostEnvironment; 
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

		var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

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

		//var homeCommunityId = "2.16.578.1.12.4.5.100.3.15";
		var homeCommunityId = run.HomeCommunityId; 

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

		void ConfigureDocument2(NorskHelsenettUpload.Ihe.Mhd.Models.Document document)
		{
			if (test.SecurityLabels.Count > 0)
			{
				document.Metadata.ConfidentialityCodeList = new List<string>(); 

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

					var confidentialityCodeCoding = ConfidentialityCodeList.ConfidentialityCodes.FirstOrDefault(coding => coding.Code == securityLabel);

					document.Metadata.WithConfidentialityCode(confidentialityCodeCoding);
				}
			}
		}

		bundleRequest = await _createDocumentService.GenerateRandomTestDocuments(bundleRequest, sessionData, 1, ConfigureDocument, ConfigureDocument2);

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

		var httpClient = _httpClientFactory.CreateClient(DokumentlagerTestHttpClientName);

		if (test.Lookup != null)
		{
			await DoLookup(run, documentReferenceId, test.Lookup);
		}

		if (test.Events != null)
		{
			if (test.Events.IsTrySimulateDeath)
			{				
				test.Events.SimulateDeathStatus = DokumentlagerTestCaseStatus.Running;
				_store.Update(run);

				var deceasedDate = DateTime.Now; 
				
				var postSimulateDeathApiUrl = 
					$"{_apiEnvironment.RootUrl}/simulate-persontjenesten-events/person-death-results-in-scheduled-document-delete?nin={sessionData.PatientId}&year={deceasedDate.Year}&month={deceasedDate.Month}&day={deceasedDate.Day}";

				var postSimulateDeathResponse = await httpClient.PostAsync(postSimulateDeathApiUrl, null);

				if (postSimulateDeathResponse.IsSuccessStatusCode)
				{
					var responseContent = await postSimulateDeathResponse.Content.ReadAsStringAsync();

					test.Events.SimulateDeathStatus = DokumentlagerTestCaseStatus.Success;
					test.Events.SimulateDeathIsSuccess = true;				
				}
				else
				{					
					test.Events.SimulateDeathStatus = DokumentlagerTestCaseStatus.Failed;
					test.Events.SimulateDeathIsSuccess = false;
				}

				_store.Update(run);
			}

			if (test.Events.IsTrySimulateAddressProtection)
			{
				test.Events.SimulateAddressProtectionStatus = DokumentlagerTestCaseStatus.Running;
				_store.Update(run);				

				var postSimulateAddressProtectionApiUrl =
					$"{_apiEnvironment.RootUrl}/simulate-persontjenesten-events/person-set-address-protection?nin={sessionData.PatientId}";

				var postSimulateAddressProtectionResponse = await httpClient.PostAsync(postSimulateAddressProtectionApiUrl, null);

				if (postSimulateAddressProtectionResponse.IsSuccessStatusCode)
				{
					var responseContent = await postSimulateAddressProtectionResponse.Content.ReadAsStringAsync();

					test.Events.SimulateAddressProtectionStatus = DokumentlagerTestCaseStatus.Success;
					test.Events.SimulateAddressProtectionIsSuccess = true;
				}
				else
				{
					test.Events.SimulateAddressProtectionStatus = DokumentlagerTestCaseStatus.Failed;
					test.Events.SimulateAddressProtectionIsSuccess= false;
				}

				_store.Update(run);
			}
		}

		if (test.LookupAfterEvents != null)
		{
			await DoLookup(run, documentReferenceId, test.LookupAfterEvents);
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

		if (test.Events != null)
		{
			if (test.Events.IsTrySimulateDeath)
			{
				result.Success = result.Success && test.Events.SimulateDeathIsSuccess;
			}

			if (test.Events.IsTrySimulateAddressProtection)
			{
				result.Success = result.Success && test.Events.SimulateAddressProtectionIsSuccess;
			}
		}

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

	private async System.Threading.Tasks.Task DoLookup(TestRunInfo run, Guid? documentReferenceId, DokumentlagerTestCaseLookup lookup)
	{
		var lookupDokumentlager = lookup.LookupDokumentlager;
		var lookupAPI = lookup.LookupAPI;
		var lookupKJ = lookup.LookupKJ;
		var lookupHelsenorge = lookup.LookupHelsenorge;

		var test = run.Test;
		var sessionData = run.SessionData;

		var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

		var homeCommunityId = run.HomeCommunityId;

		var httpClient = _httpClientFactory.CreateClient(DokumentlagerTestHttpClientName);

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

				var getDocumentStatusApiUrl = $"{_apiEnvironment.RootUrl}/registry-repository/document-status?home={homeCommunityId}&document={documentReferenceId}";

				var getDocumentStatusResponse = await httpClient.GetAsync(getDocumentStatusApiUrl);

				if (getDocumentStatusResponse.IsSuccessStatusCode)
				{
					var responseContent = await getDocumentStatusResponse.Content.ReadAsStringAsync();

					var documentStatusResult = JsonSerializer.Deserialize<DocumentStatusResult>(responseContent);

					if (lookupDokumentlager.IsTryRetrieveDocumentReference)
					{
						lookupDokumentlager.RetrieveDocumentReferenceStatus = documentStatusResult.document.savedToRegistry ? LookupDocumentReferenceStatus.Visible : LookupDocumentReferenceStatus.NotVisible; 

						//lookupDokumentlager.RetrieveDocumentReferenceIsSuccess = documentStatusResult.document.savedToRegistry;
						//lookupDokumentlager.RetrieveDocumentReferenceStatus = lookupDokumentlager.RetrieveDocumentReferenceIsSuccess ? LookupDocumentReferenceStatus.Visible : LookupDocumentReferenceStatus.NotVisible;
					}

					if (lookupDokumentlager.IsTryOpenDocument)
					{
						lookupDokumentlager.OpenDocumentStatus = documentStatusResult.document.savedToRepository ? LookupDocumentStatus.OpenDocumentSuccess : LookupDocumentStatus.OpenDocumentFailed;

						//lookupDokumentlager.OpenDocumentIsSuccess = documentStatusResult.document.savedToRepository;
						//lookupDokumentlager.OpenDocumentStatus = lookupDokumentlager.OpenDocumentIsSuccess ? LookupDocumentStatus.OpenDocumentSuccess : LookupDocumentStatus.OpenDocumentFailed;
					}
				}
				else
				{
					//lookupDokumentlager.RetrieveDocumentReferenceIsSuccess = false;
					lookupDokumentlager.IsTryOpenDocument = false;

					lookupDokumentlager.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.NotVisible;
					lookupDokumentlager.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentFailed;
				}

				if (lookupDokumentlager.RetrieveDocumentReferenceExpectVisible)
				{
					lookupDokumentlager.RetrieveDocumentReferenceIsSuccess = lookupDokumentlager.RetrieveDocumentReferenceStatus == LookupDocumentReferenceStatus.Visible;
				}
				else
				{
					lookupDokumentlager.RetrieveDocumentReferenceIsSuccess = lookupDokumentlager.RetrieveDocumentReferenceStatus == LookupDocumentReferenceStatus.NotVisible;
				}

				if (lookupDokumentlager.OpenDocumentExpectSuccess)
				{
					lookupDokumentlager.OpenDocumentIsSuccess = lookupDokumentlager.OpenDocumentStatus == LookupDocumentStatus.OpenDocumentSuccess; 
				}
				else
				{
					lookupDokumentlager.OpenDocumentIsSuccess = lookupDokumentlager.OpenDocumentStatus == LookupDocumentStatus.OpenDocumentFailed;
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
					sessionData.PjdAccessToken!,
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
						//var title = uploadedDocumentReference.Description;
						var contentComponent = uploadedDocumentReference.Content.FirstOrDefault();
						var attachment = contentComponent!.Attachment;

						var title = attachment.Title;

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

					var apiGetDocumentResult = await ApiCaller.CallApi(HttpMethod.Get, retrieveDocumentUri.ToString(), sessionData.PjdAccessToken!, document: true, headerKeyValues: headerKeyValues);

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
			//KjernejournalDocumentList kjernejournalDocumentList = KjernejournalDocumentList.CreateRandomDocumentList(5); // Simulate retrieving document list from Kjernejournal. 

			Uri? kjRetrieveDocumentUri = null;
			string? kjRetrieveDocumentUniqueId = null;

			/*if (lookupKJ.IsTryRetrieveDocumentReference)
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

			_store.Update(run); */

			if (lookupKJ.IsTryRetrieveDocumentReference)
			{
				lookupKJ.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.Running;
				_store.Update(run);

				var iti38RequestFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, @"DokumentlagerTest\Data\Kjernejournal-portal\iti38-request.xml");
				var iti38SoapEnvelope = File.ReadAllText(iti38RequestFilePath);

				var kjSamlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, @"DokumentlagerTest\Data\Kjernejournal-portal\SamlToken_KJ01.xml");
				var kjSamlToken = File.ReadAllText(kjSamlFilePath);

				var crossGatewayQuery = GetSoapEnvelopeWithSamlToken(iti38SoapEnvelope, kjSamlToken);

				//var forwardCrossGatewayQueryApiUrl = $"{_apiEnvironment.RootUrl}/registry-repository/forward-cross-gateway-query?home={homeCommunityId}&patientIdentifier={sessionData.PatientId}&subjectId={sessionData.PersonId}";
				var forwardCrossGatewayQueryApiUrl = $"{_apiEnvironment.RootUrl}/registry-repository/forward-to-responding-gateway-service?home={homeCommunityId}&patientIdentifier={sessionData.PatientId}&subjectId={sessionData.PersonId}";
				//var forwardCrossGatewayQueryApiUrl = $"{_apiEnvironment.RootUrl}/registry-repository/forward-cross-gateway-query?home={homeCommunityId}&patientIdentifier=20738506678";

				var forwardCrossGatewayQueryResponse = await httpClient.PostAsync(forwardCrossGatewayQueryApiUrl, new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

				if (forwardCrossGatewayQueryResponse.IsSuccessStatusCode)
				{
					var responseContent = await forwardCrossGatewayQueryResponse.Content.ReadAsStringAsync();

					var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
					var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(await forwardCrossGatewayQueryResponse.Content.ReadAsStreamAsync());

					var extrinsincObjects = firstResponseSoap?.Body.AdhocQueryResponse?.RegistryObjectList?.OfType<ExtrinsicObjectType>().ToList();

					var kjDocumentReference = extrinsincObjects?.FirstOrDefault(eo => eo.Id == documentReferenceId.ToString());

					var count = extrinsincObjects?.Count() ?? 0;

					if (kjDocumentReference != null)
					{
						kjRetrieveDocumentUniqueId = GetUniqueId(kjDocumentReference);

						var title = kjDocumentReference.Name.GetFirstValue();

						if (title.Contains("***"))
						{
							lookupKJ.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.VisibleWithObfuscation;
						}
						else
						{
							lookupKJ.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.Visible;
						}
					}
					else
					{
						lookupKJ.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.NotVisible;
					}
				}
				else
				{
					lookupKJ.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.NotVisible;
					lookupKJ.RetrieveDocumentReferenceIsSuccess = false;
				}

				/*var hnDocumentReference = helsenorgeDocumentList.Dokumenter?.FirstOrDefault(dr => dr.Id == documentReferenceId.ToString());

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
				}*/

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

				if (kjRetrieveDocumentUniqueId == null)
				{
					lookupKJ.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentFailed;
					lookupKJ.OpenDocumentIsSuccess = !lookupKJ.OpenDocumentExpectSuccess;
				}
				else
				{
					var iti39RequestFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, @"DokumentlagerTest\Data\Kjernejournal-portal\iti39-iti40-request-kj.xml");
					var iti39SoapEnvelope = File.ReadAllText(iti39RequestFilePath);

					var kjSamlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, @"DokumentlagerTest\Data\Kjernejournal-portal\SamlToken_KJ01.xml");
					var kjSamlToken = File.ReadAllText(kjSamlFilePath);

					var crossGatewayQuery = GetSoapEnvelopeWithSamlToken(iti39SoapEnvelope, kjSamlToken);

					var forwardCrossGatewayQueryApiUrl = $"{_apiEnvironment.RootUrl}/registry-repository/forward-to-responding-gateway-service?home={homeCommunityId}&patientIdentifier={sessionData.PatientId}&subjectId={sessionData.PersonId}&documentUniqueId={kjRetrieveDocumentUniqueId}";

					var forwardCrossGatewayQueryResponse = await httpClient.PostAsync(forwardCrossGatewayQueryApiUrl, new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

					if (forwardCrossGatewayQueryResponse.IsSuccessStatusCode)
					{
						var responseContent = await forwardCrossGatewayQueryResponse.Content.ReadAsStringAsync();

						var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
						var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(await forwardCrossGatewayQueryResponse.Content.ReadAsStreamAsync());

						var document = firstResponseSoap.Body.RetrieveDocumentSetResponse?.DocumentResponse?.FirstOrDefault();

						if (document != null)
						{
							lookupKJ.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentSuccess;
						}
						else
						{
							lookupKJ.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentFailed;
						}
					}
					else
					{
						lookupKJ.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentFailed;
					}

					/*var getHelsenorgeDocumentUrl = "https://google.com";

					var getDocumentStatusResponse = await httpClient.GetAsync(getHelsenorgeDocumentUrl);

					lookupHelsenorge.OpenDocumentStatus = getDocumentStatusResponse.IsSuccessStatusCode ? LookupDocumentStatus.OpenDocumentSuccess : LookupDocumentStatus.OpenDocumentFailed;*/

					lookupKJ.OpenDocumentIsSuccess = lookupKJ.OpenDocumentExpectSuccess && lookupKJ.OpenDocumentStatus == LookupDocumentStatus.OpenDocumentSuccess
						|| (!lookupKJ.OpenDocumentExpectSuccess && lookupKJ.OpenDocumentStatus == LookupDocumentStatus.OpenDocumentFailed);
				}

				/*if (kjRetrieveDocumentUri == null)
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
				} */

				_store.Update(run);
			}
		}

		// From Helsenorge
		if (lookupHelsenorge != null)
		{
			//HelsenorgeDocumentList helsenorgeDocumentList = HelsenorgeDocumentList.CreateRandomDocumentList(5); // Simulate retrieving document list from Helsenorge. 

			Uri? hnRetrieveDocumentUri = null;
			string? hnRetrieveDocumentUniqueId = null;

			if (lookupHelsenorge.IsTryRetrieveDocumentReference)
			{
				lookupHelsenorge.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.Running;
				_store.Update(run);

				var iti38RequestFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, @"DokumentlagerTest\Data\Helsenorge\iti38-request.xml");
				var iti38SoapEnvelope = File.ReadAllText(iti38RequestFilePath);

				var helsenorgeSamlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, @"DokumentlagerTest\Data\Helsenorge\SamlToken_HN01.xml");
				var helsenorgeSamlToken = File.ReadAllText(helsenorgeSamlFilePath);

				var crossGatewayQuery = GetSoapEnvelopeWithSamlToken(iti38SoapEnvelope, helsenorgeSamlToken);

				//var forwardCrossGatewayQueryApiUrl = $"{_apiEnvironment.RootUrl}/registry-repository/forward-cross-gateway-query?home={homeCommunityId}&patientIdentifier={sessionData.PatientId}";
				var forwardCrossGatewayQueryApiUrl = $"{_apiEnvironment.RootUrl}/registry-repository/forward-to-responding-gateway-service?home={homeCommunityId}&patientIdentifier={sessionData.PatientId}";
				//var forwardCrossGatewayQueryApiUrl = $"{_apiEnvironment.RootUrl}/registry-repository/forward-cross-gateway-query?home={homeCommunityId}&patientIdentifier=20738506678";

				var forwardCrossGatewayQueryResponse = await httpClient.PostAsync(forwardCrossGatewayQueryApiUrl, new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

				if (forwardCrossGatewayQueryResponse.IsSuccessStatusCode)
				{
					var responseContent = await forwardCrossGatewayQueryResponse.Content.ReadAsStringAsync();

					var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
					var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(await forwardCrossGatewayQueryResponse.Content.ReadAsStreamAsync());

					var extrinsincObjects = firstResponseSoap?.Body.AdhocQueryResponse?.RegistryObjectList?.OfType<ExtrinsicObjectType>().ToList();

					var hnDocumentReference = extrinsincObjects?.FirstOrDefault(eo => eo.Id == documentReferenceId.ToString());

					var count = extrinsincObjects?.Count() ?? 0;

					if (hnDocumentReference != null)
					{
						hnRetrieveDocumentUniqueId = GetUniqueId(hnDocumentReference);

						var title = hnDocumentReference.Name.GetFirstValue();

						if (title.Contains("***"))
						{
							lookupHelsenorge.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.VisibleWithObfuscation;
						}
						else
						{
							lookupHelsenorge.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.Visible;
						}
					}
					else
					{
						lookupHelsenorge.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.NotVisible;
					}

					/*if (lookupHelsenorge.RetrieveDocumentReferenceExpectVisible)
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
					}*/

					//var excpectedRegistryObjects = RegistryContent.Where(rc => !rc.DocumentEntry.ConfidentialityCode.Any(ccode => BusinessLogicFilters.CitizenConfidentialityCodesToObfuscate.Contains((ccode.Code!, ccode.CodeSystem!)))).ToArray();

					// Cleanup
					/*await NukeRegistryRepository();
					_policyRepositoryService.DeleteAllPolicies();

					Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
					Assert.Equal(0, firstResponseSoap?.Body.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);
					Assert.Equal(0, firstResponseSoap?.Body.AdhocQueryResponse?.RegistryErrorList?.RegistryError?.Length ?? 0);*/

				}
				else
				{
					lookupHelsenorge.RetrieveDocumentReferenceStatus = LookupDocumentReferenceStatus.NotVisible;
					lookupHelsenorge.RetrieveDocumentReferenceIsSuccess = false;
				}

				/*var hnDocumentReference = helsenorgeDocumentList.Dokumenter?.FirstOrDefault(dr => dr.Id == documentReferenceId.ToString());

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
				}*/

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

				if (hnRetrieveDocumentUniqueId == null)
				{
					lookupHelsenorge.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentFailed;
					lookupHelsenorge.OpenDocumentIsSuccess = !lookupHelsenorge.OpenDocumentExpectSuccess;
				}
				else
				{
					var iti39RequestFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, @"DokumentlagerTest\Data\Helsenorge\iti39-iti40-request-hn.xml");
					var iti39SoapEnvelope = File.ReadAllText(iti39RequestFilePath);

					var helsenorgeSamlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, @"DokumentlagerTest\Data\Helsenorge\SamlToken_HN01.xml");
					var helsenorgeSamlToken = File.ReadAllText(helsenorgeSamlFilePath);

					var crossGatewayQuery = GetSoapEnvelopeWithSamlToken(iti39SoapEnvelope, helsenorgeSamlToken);

					var forwardCrossGatewayQueryApiUrl = $"{_apiEnvironment.RootUrl}/registry-repository/forward-to-responding-gateway-service?home={homeCommunityId}&patientIdentifier={sessionData.PatientId}&documentUniqueId={hnRetrieveDocumentUniqueId}";

					var forwardCrossGatewayQueryResponse = await httpClient.PostAsync(forwardCrossGatewayQueryApiUrl, new StringContent(crossGatewayQuery.OuterXml, Encoding.UTF8, Constants.MimeTypes.SoapXml));

					if (forwardCrossGatewayQueryResponse.IsSuccessStatusCode)
					{
						var responseContent = await forwardCrossGatewayQueryResponse.Content.ReadAsStringAsync();

						var sxmls = new SoapXmlSerializer(Constants.XmlDefaultOptions.DefaultXmlWriterSettings);
						var firstResponseSoap = sxmls.DeserializeXmlString<SoapEnvelope>(await forwardCrossGatewayQueryResponse.Content.ReadAsStreamAsync());

						var document = firstResponseSoap.Body.RetrieveDocumentSetResponse?.DocumentResponse?.FirstOrDefault();

						if (document != null)
						{
							lookupHelsenorge.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentSuccess;
						}
						else
						{
							lookupHelsenorge.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentFailed;
						}
					}
					else
					{
						lookupHelsenorge.OpenDocumentStatus = LookupDocumentStatus.OpenDocumentFailed;
					}

					/*var getHelsenorgeDocumentUrl = "https://google.com";

					var getDocumentStatusResponse = await httpClient.GetAsync(getHelsenorgeDocumentUrl);

					lookupHelsenorge.OpenDocumentStatus = getDocumentStatusResponse.IsSuccessStatusCode ? LookupDocumentStatus.OpenDocumentSuccess : LookupDocumentStatus.OpenDocumentFailed;*/

					lookupHelsenorge.OpenDocumentIsSuccess = lookupHelsenorge.OpenDocumentExpectSuccess && lookupHelsenorge.OpenDocumentStatus == LookupDocumentStatus.OpenDocumentSuccess
						|| (!lookupHelsenorge.OpenDocumentExpectSuccess && lookupHelsenorge.OpenDocumentStatus == LookupDocumentStatus.OpenDocumentFailed);
				}

				_store.Update(run);
			}
		}
	}

	private XmlDocument? GetSoapEnvelopeWithKjernejournalForskriftenSamlToken(string soapEnvelope)
	{
		var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");
		var testDataFiles = Directory.GetFiles(testDataPath);

		var integrationTestFiles = Directory.GetFiles(Path.Combine(testDataPath, "IntegrationTests"));

		var kjSamlTokenString = File.ReadAllText(integrationTestFiles.FirstOrDefault(f => f.Contains("IT_SamlToken_KJ01")));

		var doc = XDocument.Parse(kjSamlTokenString);

		XNamespace saml = "urn:oasis:names:tc:SAML:2.0:assertion";

		var attribute = doc.Descendants(saml + "Attribute")
			.FirstOrDefault(a => (string?)a.Attribute("Name") == Constants.Saml.Attribute.EhelseScope);

		if (attribute != null)
		{
			var valueElement = attribute.Element(saml + "AttributeValue");
			if (valueElement != null)
			{
				// HAYO! KJ_SCOPE
				valueElement.Value = "kjernejournalforskriften";
			}
		}
		var soapEnvelopeDocument = LoadNewXmlDocument(soapEnvelope);
		var kjSamlToken = LoadNewXmlDocument(doc.ToString());

		return GetSoapEnvelopeWithSamlToken(soapEnvelopeDocument, kjSamlToken);
	}

	private ExternalIdentifierType? GetExternalIdentifier(ExtrinsicObjectType eo, string identificationSchemeValue)
	{
		return eo.ExternalIdentifier.FirstOrDefault(s => s.IdentificationScheme.Equals(identificationSchemeValue));
	}

	private string? GetUniqueId(ExtrinsicObjectType eo)
	{
		var externalIdentifier = GetExternalIdentifier(eo, Constants.Xds.Uuids.DocumentEntry.UniqueId);
		return externalIdentifier?.Value;
	}
	
	private XmlDocument? GetSoapEnvelopeWithSamlToken(string iti39SoapEnvelope, string samlToken)
	{				
		var helsenorgeSamlToken = LoadNewXmlDocument(samlToken);
		var soapEnvelopeDocument = LoadNewXmlDocument(iti39SoapEnvelope);

		return GetSoapEnvelopeWithSamlToken(soapEnvelopeDocument, helsenorgeSamlToken);
	}
	
	private XmlDocument? GetSoapEnvelopeWithSamlToken(XmlDocument? soapEnvelopeDocument, XmlDocument? samlToken)
	{
		var nsmgr = new XmlNamespaceManager(soapEnvelopeDocument.NameTable);
		nsmgr.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
		nsmgr.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

		var securityNode = soapEnvelopeDocument.SelectSingleNode("//wsse:Security", nsmgr);

		if (securityNode != null)
		{
			var importedSamlToken = soapEnvelopeDocument.ImportNode(samlToken.DocumentElement, true);

			securityNode.AppendChild(importedSamlToken);
		}

		return soapEnvelopeDocument;
	}

	public static XmlDocument? LoadNewXmlDocument(string? fileContent)
	{
		if (string.IsNullOrWhiteSpace(fileContent)) return null;
		try
		{
			var document = new XmlDocument();
			document.LoadXml(fileContent);
			return document;
		}
		catch
		{
			return null;
		}
	}
}