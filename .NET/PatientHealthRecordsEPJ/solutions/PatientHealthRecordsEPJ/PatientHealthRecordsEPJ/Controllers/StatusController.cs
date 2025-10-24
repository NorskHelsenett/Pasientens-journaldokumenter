using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.Mapper.Xml;
using PatientHealthRecordsEPJ.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace PatientHealthRecordsEPJ.Controllers; 

public class StatusController : BaseController
{
	private readonly ILogger<HomeController> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private ISession session => _httpContextAccessor.HttpContext!.Session;
	private readonly IWebHostEnvironment _webHostEnvironment;
	private readonly IOidcClientService _oidcClientService;
	private readonly IPhrApiEnvironmentProvider _phrApiEnvironmentProvider;

	SessionData sessionData;

	public StatusController(
		IOptions<SessionOptions> sessionOptions, 
		ILogger<HomeController> logger, 
		IHttpContextAccessor httpContextAccessor, 
		IMemoryCache memoryCache, 
		IWebHostEnvironment webHostEnvironment,		
		IOidcClientService oidcClientService,
		IPhrApiEnvironmentProvider phrApiEnvironmentProvider
		) : base(sessionOptions)
	{
		_logger = logger;
		_httpContextAccessor = httpContextAccessor;
		_webHostEnvironment = webHostEnvironment;
		_oidcClientService = oidcClientService;
		_phrApiEnvironmentProvider = phrApiEnvironmentProvider;
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
		var sessionData = session.SessionData();

		if (sessionData.AccessToken == null)
		{
			return Redirect("/");
		}

		await RefreshToken();

		this.sessionData = session.SessionData();

		if (!sessionData.IsAuthenticated)
			return Redirect("/");

		var model = await GetStatus(sessionData.PatientId!);

		return View(model);
	}

	private async Task<StatusViewModel> GetStatus(
		string patient
		)
	{
		StatusViewModel model = new StatusViewModel();

		// We send the patient Id in the header as X-Patient-Id to avoid having the patient Id in the URL.             
		var headerKeyValues = new List<KeyValuePair<string, string>>()
		{
			new KeyValuePair<string, string>("nhn-patient-nin", patient),
			new KeyValuePair<string, string>("nhn-source-system", patient),
			new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()),
			new KeyValuePair<string, string>("nhn-access-basis", "UNNTAK")
		};

		var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

		var apiUrl = _apiEnvironment.StatusUrl;

		var jsonContent = System.Text.Json.JsonSerializer.Serialize(new { nin = patient });

		var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"); 

		var apiResult = await ApiCaller.CallApi("POST", apiUrl, sessionData.AccessToken!, document: false, headerKeyValues: headerKeyValues, content: content, isStatus: true);

		if (apiResult.IsSuccess)
		{
			try
			{
				StatusResult? status = JsonSerializer.Deserialize<StatusResult>(apiResult.Body, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

				// Call /statuspvk endpoint to get raw pvk settings 

				var statusPvkUrl = _apiEnvironment.StatusPvkUrl;
				apiResult = await ApiCaller.CallApi("POST", statusPvkUrl, sessionData.AccessToken!, document: false, headerKeyValues: headerKeyValues, content: content, isStatus: true);

				StatusPvkResult? statusPvk = JsonSerializer.Deserialize<StatusPvkResult>(apiResult.Body, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

				if (statusPvk != null)
				{
					statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Json = statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Json.PrettifyJson();

					statusPvk.Kjernejournal_Reservasjon_Json = statusPvk.Kjernejournal_Reservasjon_Json.PrettifyJson();
					statusPvk.Kjernejournal_Sperre_Tilgang_Alt_Helsepersonell_Json = statusPvk.Kjernejournal_Sperre_Tilgang_Alt_Helsepersonell_Json.PrettifyJson();
					statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Json = statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Json.PrettifyJson();
					statusPvk.Journaldokumenter_Sperre_Tilgang_Alt_Helsepersonell_Json = statusPvk.Journaldokumenter_Sperre_Tilgang_Alt_Helsepersonell_Json.PrettifyJson();
					statusPvk.Besokshistorikk_Sperre_Tilgang_Alt_Helsepersonell_Json = statusPvk.Besokshistorikk_Sperre_Tilgang_Alt_Helsepersonell_Json.PrettifyJson();
					statusPvk.Besokshistorikk_Blokkere_For_Besokshistorikk_og_Journaldokumenter_Json = statusPvk.Besokshistorikk_Blokkere_For_Besokshistorikk_og_Journaldokumenter_Json.PrettifyJson();

					statusPvk.Kjernejournal_Reservasjon_Active = statusPvk.Kjernejournal_Reservasjon_Json?.ToLower().Contains("status\": \"res\""); 
					statusPvk.Kjernejournal_Sperre_Tilgang_Alt_Helsepersonell_Active = statusPvk.Kjernejournal_Sperre_Tilgang_Alt_Helsepersonell_Json?.ToLower().Contains("status\": \"tbo\"");
					statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Active = statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Json?.ToLower().Contains("status\": \"tbo\"");
					statusPvk.Journaldokumenter_Sperre_Tilgang_Alt_Helsepersonell_Active = statusPvk.Journaldokumenter_Sperre_Tilgang_Alt_Helsepersonell_Json?.ToLower().Contains("status\": \"tbo\"");
					statusPvk.Besokshistorikk_Sperre_Tilgang_Alt_Helsepersonell_Active = statusPvk.Besokshistorikk_Sperre_Tilgang_Alt_Helsepersonell_Json?.ToLower().Contains("status\": \"tbo\"");
					statusPvk.Besokshistorikk_Blokkere_For_Besokshistorikk_og_Journaldokumenter_Active = statusPvk.Besokshistorikk_Blokkere_For_Besokshistorikk_og_Journaldokumenter_Json?.ToLower().Contains("status\": \"tbo\"");
				}
				
				model.IsSuccess = true; 
				model.Status = status;
				model.StatusPvk = statusPvk;
				model.JsonStatus = apiResult.Body;
				return model; 
			}
			catch 
			{				
				model.IsSuccess = false;
				model.ErrorMessage = "Could not get status";
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
