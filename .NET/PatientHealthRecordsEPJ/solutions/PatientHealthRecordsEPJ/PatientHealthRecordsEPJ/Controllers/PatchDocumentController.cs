using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.Models;
using PatientHealthRecordsEPJ.Services;
using System.Diagnostics;
using XcaXds.Commons.Extensions;

namespace PatientHealthRecordsEPJ.Controllers; 

public class PatchDocumentController : BaseController
{
	private readonly ILogger<HomeController> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;	
	private ISession session => _httpContextAccessor.HttpContext!.Session;
	private readonly IWebHostEnvironment _webHostEnvironment;
	private readonly IOidcClientService _oidcClientService; 
	private readonly IPhrApiEnvironmentProvider _phrApiEnvironmentProvider;

	SessionData sessionLoginResult;

	public PatchDocumentController(
		IOptions<SessionOptions> sessionOptions, 
		ILogger<HomeController> logger, 
		IHttpContextAccessor httpContextAccessor, 
		IMemoryCache memoryCache, 
		IWebHostEnvironment webHostEnvironment,		
		IOidcClientService oidcClientService,
		IPhrApiEnvironmentProvider phrApiEnvironmentProvider,
		IUserTokenService userTokenService
		) : base(sessionOptions, userTokenService)
	{
		_logger = logger;
		_httpContextAccessor = httpContextAccessor;
		_webHostEnvironment = webHostEnvironment;
		_oidcClientService = oidcClientService;
		_phrApiEnvironmentProvider = phrApiEnvironmentProvider;
	}

	public async Task<IActionResult> Index()
	{
		var loginResult = await _userTokenService.GetValidAccessTokensAsync(HttpContext);

		if (loginResult.PjdRepositoryAccessToken == null)
		{
			return Redirect("/");
		}		

		sessionLoginResult = session.SessionData();

		if (!sessionLoginResult.IsAuthenticated)
			return Redirect("/");

		var body = """
			   {
			  "resourceType": "DocumentReference",
			  "id": "e2f09811-1e4f-4050-adab-151a8644c8fa",
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

		var id = "e2f09811-1e4f-4050-adab-151a8644c8fa";
		var homeCommunityId = "2.16.578.1.12.4.5.100.3.17"; 

		var model = await PatchDocument(id, homeCommunityId, body);

		return View(model);
	}

	public async Task<IActionResult> PatchDocument(string id, string homeCommunityId, string body)
	{
		var sessionData = session.SessionData();

		id = id.NoUrn();
		homeCommunityId = homeCommunityId.NoUrn();

		var headerKeyValues = new List<KeyValuePair<string, string>>();

		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-source-system", "PJD Test EPJ"));
		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()));

		//var content = new StringContent(body, System.Text.Encoding.UTF8, "application/fhir+json");
		var content = new StringContent(body, null, "application/fhir+json");

		var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

		var apiUrl = $"{_apiEnvironment.PatchDocumentUrl}/{id}?homeCommunityId={homeCommunityId}";

		var apiResult = await ApiCaller.CallApi(HttpMethod.Patch,
			apiUrl,
			sessionData.PjdRepositoryAccessToken!,
			document: false,
			headerKeyValues: headerKeyValues,
			content: content
			);

		if (apiResult.IsSuccess)
		{
			try
			{
				var json = apiResult.Body;

				var fhirDeserializer = new FhirJsonDeserializer();

				var operationOutcome = fhirDeserializer.Deserialize<OperationOutcome>(json);

				if (operationOutcome != null)
				{
					foreach (var issue in operationOutcome.Issue)
					{
						if (issue.Severity == OperationOutcome.IssueSeverity.Error)
						{
							var firstCoding = issue.Details.Coding.FirstOrDefault();

							if (firstCoding != null)
							{
								var system = firstCoding.System;
								var code = firstCoding.Code;
								var display = firstCoding.Display;
							}
						}
					}
				}

				GetDocumentReferencesViewModel model = new GetDocumentReferencesViewModel();
				model.IsSuccess = true;
				model.DeletedDocumentReferenceId = id;
				//model.ErrorMessage = $"Could not delete document {id}";
				model.ApiResult = apiResult;
				model.Pagination = new Pagination(); // To avoid NullReferenceException in onChangePageSize() and onChangePage() in Index.cshtml
				return View("DeleteConfirmation", model);
			}
			catch (Hl7.Fhir.Serialization.DeserializationFailedException e)
			{
				foreach (var ex in e.Exceptions)
				{
					Debug.WriteLine(ex.Message);
				}

				GetDocumentReferencesViewModel model = new GetDocumentReferencesViewModel();
				model.IsSuccess = false;
				model.ErrorMessage = $"Could not delete document {id}";
				model.ApiResult = apiResult;
				model.Pagination = new Pagination(); // To avoid NullReferenceException in onChangePageSize() and onChangePage() in Index.cshtml
				return View("Index", model);
			}
		}
		else
		{
			GetDocumentReferencesViewModel model = new GetDocumentReferencesViewModel();
			model.IsSuccess = false;
			model.ErrorMessage = $"Could not delete document {id}";
			model.ApiResult = apiResult;
			model.Pagination = new Pagination(); // To avoid NullReferenceException in onChangePageSize() and onChangePage() in Index.cshtml
			return View("Index", model);
		}

		//return Redirect("/documents"); 
	}	
}
