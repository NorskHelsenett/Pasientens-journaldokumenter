using Hl7.Fhir.Model.CdsHooks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NorskHelsenettUpload;
using NorskHelsenettUpload.FluidExtensions; 
using NorskHelsenettUpload.Ihe.Mhd.Interfaces;
using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Ihe.Mhd.Services;
using NorskHelsenettUpload.Models;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.DokumentlagerTest;
using PatientHealthRecordsEPJ.Models;
using PatientHealthRecordsEPJ.Personverninnstillinger;
using PatientHealthRecordsEPJ.Services;
using System.Text;
using System.Text.Json;
using XcaXds.Commons.Models.Custom.RegistryDtos;
using static NorskHelsenettUpload.Constants.CodeSystems.Hl7.ConfidentialityCode;
using static NorskHelsenettUpload.Constants.CodeSystems.Hl7.PurposeOfUse;
using static NorskHelsenettUpload.Constants.CodeSystems.Volven.ConfidentialityCode;

namespace PatientHealthRecordsEPJ.Controllers;

//[Route("api/dokumentlager-tests")]
//[Route("DokumentlagerTest/")]
//[Route("[controller]/[action]")]
//[Route("[controller]")]
public class DokumentlagerTestController : BaseController
{
	private readonly ILogger<HomeController> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private ISession session => _httpContextAccessor.HttpContext!.Session;
	private readonly IWebHostEnvironment _webHostEnvironment;
	private readonly IOidcClientService _oidcClientService;
	private readonly IPhrApiEnvironmentProvider _phrApiEnvironmentProvider;
	private readonly IBundleCreator _bundleCreator;
	private readonly OrganizationService _organizationService;
	private readonly PhrEpjOptions _phrEpjOptions;
	private readonly ICreateDocumentService _createDocumentService;
	private readonly ProvideDocumentBundleApiService _provideDocumentsBundleApiService;

	private readonly ITestRunStore _store;
	private readonly IBackgroundTaskQueue _queue;
	private readonly IServiceScopeFactory _scopeFactory;


	SessionData sessionData;

	public DokumentlagerTestController(
		IOptions<SessionOptions> sessionOptions, 
		ILogger<HomeController> logger, 
		IHttpContextAccessor httpContextAccessor, 
		IMemoryCache memoryCache, 
		IWebHostEnvironment webHostEnvironment,		
		IOidcClientService oidcClientService,
		IPhrApiEnvironmentProvider phrApiEnvironmentProvider,
		IBundleCreator bundleCreator,
		OrganizationService organizationService,
		PhrEpjOptions phrEpjOptions,
		ICreateDocumentService createDocumentService,
		ProvideDocumentBundleApiService provideDocumentsBundleApiService,
		ITestRunStore store,
		IBackgroundTaskQueue queue,
		IServiceScopeFactory scopeFactory,
		IUserTokenService userTokenService
		) : base(sessionOptions, userTokenService)
	{
		_logger = logger;
		_httpContextAccessor = httpContextAccessor;
		_webHostEnvironment = webHostEnvironment;
		_oidcClientService = oidcClientService;
		_phrApiEnvironmentProvider = phrApiEnvironmentProvider;
		_bundleCreator = bundleCreator;
		_organizationService = organizationService;
		_phrEpjOptions = phrEpjOptions;
		_createDocumentService = createDocumentService;
		_provideDocumentsBundleApiService = provideDocumentsBundleApiService;
		_store = store;
		_queue = queue;
		_scopeFactory = scopeFactory;
	}

	public async Task<IActionResult> Index(string? homeCommunityId = null)
	{
		var sessionData = await _userTokenService.GetValidAccessTokensAsync(HttpContext);

		if (sessionData.PjdRepositoryAccessToken == null)
		{
			return Redirect("/");
		}

		if (sessionData.SourcePatientInfo == null) 
		{
			return RedirectToAction(nameof(PatientController.Pasientvalg), "Patient");
		}		

		this.sessionData = session.SessionData();

		if (!sessionData.IsAuthenticated)
			return Redirect("/");

		sessionData.DokumentlagerTestStatus.Status = DokumentlagerTestStatusEnum.Stopped;
		sessionData.DokumentlagerTests = new DokumentlagerTests();
		_store.Clear(); 

		sessionData.SaveToSession(session);

		var model = await GetTests(sessionData.PatientId!);

		model.TestsJson = JsonSerializer.Serialize(model.Tests, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

		model.SelectedHomeCommunityId = homeCommunityId;

		return View(model);
	}

	public async Task<bool> Reset()
	{
		var sessionData = session.SessionData();

		var tests = sessionData.DokumentlagerTests;

		sessionData.DokumentlagerTestStatus.Status = DokumentlagerTestStatusEnum.Stopped;
		sessionData.DokumentlagerTests = new DokumentlagerTests(); 

		sessionData.SaveToSession(session); 
	
		return true;
	}

	public async Task<bool> Pause()
	{
		var sessionData = session.SessionData();

		var tests = sessionData.DokumentlagerTests;

		sessionData.DokumentlagerTestStatus.Status = DokumentlagerTestStatusEnum.Paused; 		

		sessionData.SaveToSession(session);

		return true;
	}

	public async Task<bool> Continue()
	{
		var sessionData = session.SessionData();

		var tests = sessionData.DokumentlagerTests;

		sessionData.DokumentlagerTestStatus.Status = DokumentlagerTestStatusEnum.Running;	
		sessionData.SaveToSession(session);

		return true;
	}	

	/*public async Task<DokumentlagerTestCase> Status(int groupIndex, int testIndex)
	{
		var sessionData = session.SessionData();

		var tests = sessionData.DokumentlagerTests;

		var testGroup = tests.TestGroups[groupIndex];
		var test = testGroup.TestCases[testIndex];		

		return test;
	}*/

	//[HttpGet("status/{runId}")]
	//[HttpGet("{runId}")]
	public IActionResult Status(string runId)
	{
		if (!_store.TryGet(runId, out var run) || run is null)
			return NotFound();

		return Ok(new
		{
			runId = run.RunId,
			groupIndex = run.GroupIndex,
			testIndex = run.TestIndex,
			status = run.Status.ToString(),
			result = run.Result,
			error = run.Error,
			createdUtc = run.CreatedUtc
		});
	}
	
	public IActionResult RenderTest(string runId)
	{
		if (!_store.TryGet(runId, out var run) || run is null)
			return NotFound();

		DokumentlagerTestCaseViewModel model = new DokumentlagerTestCaseViewModel();

		model.Run = run; 

		//var sessionData = session.SessionData();

		//var tests = sessionData.DokumentlagerTests;

		//var testGroup = tests.TestGroups[groupIndex];
		//var test = testGroup.TestCases[testIndex];

		//model.Test = test;
		model.Test = run.Test;
		model.GroupIndex = run.GroupIndex;
		model.TestIndex = run.TestIndex;

		return View("_RenderTestPartial", model);
	}

	public async Task<IActionResult> RenderTotalResult()
	{ 
		//var sessionData = await _userTokenService.GetValidAccessTokensAsync(HttpContext);		

		this.sessionData = session.SessionData();		

		var model = await GetTests(sessionData.PatientId!);

		//var runs = _store.GetAllRuns();
		model.Runs = _store.GetAllRuns().ToList(); 

		return View("_RenderTotalResultPartial", model);
	}

	private async Task<DokumentlagerTestViewModel> GetTests(
		string patient
		)
	{
		DokumentlagerTestViewModel model = new DokumentlagerTestViewModel();

		var sessionData = session.SessionData();

		//sessionData.DokumentlagerTests.TestGroups.First().TestCases = [sessionData.DokumentlagerTests.TestGroups.First().TestCases.Last()];

		//sessionData.SaveToSession(session); 

		var tests = sessionData.DokumentlagerTests;
		
		//tests.TestGroups.First().TestCases = [tests.TestGroups.First().TestCases.First()];

		foreach (var group in tests.TestGroups)
		{
			foreach (var test in group.TestCases)
			{
				try
				{
					//var result = await test.RunTest(patient, sessionData.AccessToken!, _phrApiEnvironmentProvider.GetPhrApiEnvironment());
					//var result = await RunTest(test, patient);
					//test.Result = result;

					test.Result = Random.Shared.Next(Random.Shared.Next(0, 2)) == 0
						? new DokumentlagerTestResult { Success = true }
						: new DokumentlagerTestResult { Success = false, ErrorMessage = "Simulated error" };
				}
				catch (Exception ex)
				{
					//test.Result = new ApiResult { IsSuccess = false, ErrorMessage = ex.Message };
				}
			}
		}

		model.Tests = tests;

		model.IsSuccess = true; 
		
		return model;
	}

	//[HttpPost("run")]
	[HttpPost]
	public async Task<IActionResult> Run([FromQuery] string homeCommunityId, [FromQuery] int groupIndex, [FromQuery] int testIndex)
	{
		var sessionData = session.SessionData();
		var patientId = sessionData.PatientId;

		if (string.IsNullOrWhiteSpace(patientId))
			return BadRequest("PatientId missing from session.");

		var testGroup = sessionData.DokumentlagerTests.TestGroups[groupIndex];
		var test = testGroup.TestCases[testIndex];

		var run = _store.Create(homeCommunityId, test, sessionData,groupIndex, testIndex, patientId);

		await _queue.QueueAsync(async cancellationToken =>
		{
			using var scope = _scopeFactory.CreateScope();
			var runner = scope.ServiceProvider.GetRequiredService<IDokumentlagerTestRunner>();
			await runner.RunTestAsync(run.RunId, cancellationToken);
		});

		return Ok(new
		{
			runId = run.RunId,
			status = run.Status.ToString()
		});
	}

	private async Task<DokumentlagerTestResult> RunTest(DokumentlagerTestCase test, string patient)
	{
		var sessionData = session.SessionData();

		test.Status = DokumentlagerTestCaseStatus.Running; 
		sessionData.SaveToSession(session);

		var homeCommunityId = "2.16.578.1.12.4.5.100.3.15"; 

		DokumentlagerTestResult result = new DokumentlagerTestResult()
		{
			Success = true
		};

		// Create new submission and documents

		var bundleRequest = new ProvideDocumentBundleRequest();

		var submissionTitle = $"SubmissionSet-{sessionData.PatientId}-{DateTime.Now.ToString()}";

		bundleRequest.Submission
			.WithUniqueId(Guid.NewGuid())
			.WithAvailabilityStatus(DocumentSubmissionMetadata.AvailabilityStatusApproved)
			.WithSubmissionTime(DateTime.Now)
			.WithSourceId("1.1") // TODO: what to use as sourceId? For now just a hardcoded dummy. 
			.WithPatientId(sessionData.PatientId)
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

		bundleRequest = await _createDocumentService.GenerateRandomTestDocuments(bundleRequest, session.SessionData(), 3, ConfigureDocument);

		// Upload document

		var bundleDocument = _bundleCreator.CreateFhirBundle(bundleRequest, "Comprehensive");

		var validate = false; 

		var apiResult = await _provideDocumentsBundleApiService.ProvideBundle.ProvideDocumentBundleAsync(bundleDocument, validate);

		test.Status = DokumentlagerTestCaseStatus.Success;
		test.Result.Success = true; 
		
		sessionData.SaveToSession(session);

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


		// Get document list
		// Get document 

		System.Diagnostics.Debug.WriteLine($"Test '{test.Name}' completed with result: {(result.Success ? "Success" : "Failure")}");

		return result; 		
	}
}
