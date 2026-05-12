using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.Models;
using PatientHealthRecordsEPJ.Services;
using System.Text.Json;

namespace PatientHealthRecordsEPJ.Controllers; 

public class AtnaLogController : BaseController
{
	private readonly ILogger<HomeController> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private ISession session => _httpContextAccessor.HttpContext!.Session;
	private readonly IWebHostEnvironment _webHostEnvironment;
	private readonly IOidcClientService _oidcClientService;
	private readonly IPhrApiEnvironmentProvider _phrApiEnvironmentProvider;	

	SessionData sessionData;

	//const int DEFAULT_PAGE_SIZE = 15;
	const int DEFAULT_PAGE_SIZE = 10;

	public AtnaLogController(
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
		
	public async Task<IActionResult> Index(int page = 1, int pageSize = DEFAULT_PAGE_SIZE, string sort = "-date", string? homeCommunityId = null)
	{
		var sessionData = await _userTokenService.GetValidAccessTokensAsync(HttpContext); 

		if (sessionData.PjdServiceAccessToken == null)
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

		//var homeCommunityId = "1";
		//var repositoryUniqueId = "";
		var creation = new List<string>();
		//string? patient = null;
		string? source = null;
		string? sourceIdentifier = null;

		if (!string.IsNullOrWhiteSpace(homeCommunityId))
		{
			// Both of these are valid (we can send both system|value or only |value)

			//sourceIdentifier = $"urn:ietf:rfc:3986|urn:oid:{homeCommunityId}";
			sourceIdentifier = $"|urn:oid:{homeCommunityId}";
		}
		
		var model = await GetAtnaLog(homeCommunityId, creation, sessionData.PatientId!, source, sourceIdentifier, page, pageSize, sort);

		model.SelectedHomeCommunityId = homeCommunityId;

		return View(model);
	}

	private async Task<AtnaLogViewModel> GetAtnaLog(
		string? homeCommunityId,
		//string? repositoryUniqueId,
		List <string>? creation,
		string? patient,
		string? source,
		string? sourceIdentifier,
		int pageIndex = 1,
		int pageSize = DEFAULT_PAGE_SIZE,
		string sort = "-date"
		//string query = ""
		)
	{
		AtnaLogViewModel model = new AtnaLogViewModel();

		var query = new List<KeyValuePair<string, string>>();
		
		if (homeCommunityId != null)
		{
			query.Add(new("home", homeCommunityId));
		}

		//if (repositoryUniqueId != null)
		//{
		//	query.Add(new("repository", repositoryUniqueId));
		//}

		if (patient != null)
		{
			query.Add(new("patient.identifier", patient));
		}

		if (source != null)
		{
			query.Add(new("source", source));
		}

		if (sourceIdentifier != null)
		{
			query.Add(new("source:identifier", sourceIdentifier));
		}

		if (creation != null)
		{
			foreach (var d in creation)
			{
				query.Add(new("date", d));
			}
		}
	
		var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

		var apiUrl = _apiEnvironment.AtnaLogUrl;

		var atnaLogUrl = QueryHelpers.AddQueryString(apiUrl, query!);
		
		var apiResult = await ApiCaller.CallApi(HttpMethod.Get, atnaLogUrl, sessionData.PjdServiceAccessToken!, document: false, isReadBodyAsString: true);		

		if (apiResult.IsSuccess)
		{
			try
			{
				var json = apiResult.Body;

				var debugSaveToDisk = Environment.GetEnvironmentVariable("DEBUG_SAVE_TO_DISK");

				if (debugSaveToDisk == "true")
				{
					// Save responses to disk for debugging purposes. 
					var tempPath = System.IO.Path.GetTempPath() + @"_PJD_EPJ\API_ATNA_LOG_RESPONSE";

					if (!Directory.Exists(tempPath))
					{
						Directory.CreateDirectory(tempPath);
					}

					var filename = $"api_atna_log_response_{DateTime.Now:yyMMddHHmmss}.json";

					System.IO.File.WriteAllText(@$"{tempPath}\{filename}", json);
				}


				var serializerOptions = new JsonSerializerOptions().ForFhir(ModelInfo.ModelInspector).Pretty();
				var bundle = JsonSerializer.Deserialize<Bundle>(json, serializerOptions);
				
				model.Entries = bundle!.Entry!
					.OrderByDescending(x => (x.Resource as AuditEvent).Recorded!.Value.ToLocalTime())
					.Skip((pageIndex - 1) * pageSize)
					.Take(pageSize)
					.ToList();	

				model.Bundle = bundle;

				model.IsSuccess = true;
				model.JsonAuditEvents = apiResult.Body;

				model.Pagination = Pagination.Create(bundle.Total ?? 0, pageIndex, pageSize);

				model.SortExpression = sort;

				return model; 
			}
			catch 
			{				
				model.IsSuccess = false;
				model.ErrorMessage = "No ATNA log entries found";
				return model;
			}
		}
		else
		{
			model.IsSuccess = false;
			model.ErrorMessage = "Could not get status";
			return model;
		}		
	}	
}
