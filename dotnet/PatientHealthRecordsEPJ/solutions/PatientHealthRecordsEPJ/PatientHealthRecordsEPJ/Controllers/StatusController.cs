using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.Mapper.Xml;
using PatientHealthRecordsEPJ.Models;
using PatientHealthRecordsEPJ.Personverninnstillinger;
using PatientHealthRecordsEPJ.Services;
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
		var sessionData = await _userTokenService.GetValidAccessTokensAsync(HttpContext);

		if (sessionData.PjdAccessToken == null)
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

		var jsonContent = JsonSerializer.Serialize(new { nin = patient });

		var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"); 

		var apiResult = await ApiCaller.CallApi(HttpMethod.Post, apiUrl, sessionData.PjdAccessToken!, document: false, headerKeyValues: headerKeyValues, content: content, isReadBodyAsString: true);

		if (apiResult.IsSuccess)
		{
			try
			{
				StatusResult? status = JsonSerializer.Deserialize<StatusResult>(apiResult.Body, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

				model.JsonStatus = apiResult.Body;

				// Call /statuspvk endpoint to get raw pvk settings 

				var statusPvkUrl = _apiEnvironment.StatusPvkUrl;
				apiResult = await ApiCaller.CallApi(HttpMethod.Post, statusPvkUrl, sessionData.PjdAccessToken!, document: false, headerKeyValues: headerKeyValues, content: content, isReadBodyAsString: true);

				StatusPvkResult? statusPvk = JsonSerializer.Deserialize<StatusPvkResult>(apiResult.Body, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

				if (statusPvk != null)
				{
					statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Json = statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Json.PrettifyJson();

					statusPvk.Kjernejournal_Reservasjon_Json = statusPvk.Kjernejournal_Reservasjon_Json.PrettifyJson();
					statusPvk.Kjernejournal_Sperre_Tilgang_Alt_Helsepersonell_Json = statusPvk.Kjernejournal_Sperre_Tilgang_Alt_Helsepersonell_Json.PrettifyJson();
					statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Json = statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Json.PrettifyJson();
					statusPvk.Journaldokumenter_Sperre_Tilgang_Alt_Helsepersonell_Json = statusPvk.Journaldokumenter_Sperre_Tilgang_Alt_Helsepersonell_Json.PrettifyJson();					
					statusPvk.Besokshistorikk_Blokkere_For_Besokshistorikk_og_Journaldokumenter_Json = statusPvk.Besokshistorikk_Blokkere_For_Besokshistorikk_og_Journaldokumenter_Json.PrettifyJson();

					statusPvk.Kjernejournal_Reservasjon_Active = statusPvk.Kjernejournal_Reservasjon_Json?.ToLower().Contains("status\": \"res\""); 
					statusPvk.Kjernejournal_Sperre_Tilgang_Alt_Helsepersonell_Active = statusPvk.Kjernejournal_Sperre_Tilgang_Alt_Helsepersonell_Json?.ToLower().Contains("status\": \"tbo\"");
					statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Active = statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Json?.ToLower().Contains("status\": \"tbo\"");
					statusPvk.Journaldokumenter_Sperre_Tilgang_Alt_Helsepersonell_Active = statusPvk.Journaldokumenter_Sperre_Tilgang_Alt_Helsepersonell_Json?.ToLower().Contains("status\": \"tbo\"");
					statusPvk.Besokshistorikk_Blokkere_For_Besokshistorikk_og_Journaldokumenter_Active = statusPvk.Besokshistorikk_Blokkere_For_Besokshistorikk_og_Journaldokumenter_Json?.ToLower().Contains("status\": \"tbo\"");

					if (!string.IsNullOrEmpty(statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Json))
					{
						var Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Settings = JsonSerializer.Deserialize<PersonvernDataType>(statusPvk.Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Json);

						if (Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Settings != null)
						{
							var settings = Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Settings; 
							
							if (settings.Status.ToLower() == "tbo")
							{
								var omfangElementer = settings.TilgangsbegrensningMetadata?.TbFasteMetadata?.OmfangElementer; 

								if (omfangElementer?.Any(omfang => omfang.OmfangKode == "BL" && omfang.LogiskOmfang == "Angitte") == true)
								{
									var navngittePersoner = settings.TilgangsbegrensningMetadata?.TbInnbyggerMetadata?.DetaljertAngivelse.NavngittHelsepersoner;

									model.NavngittHelsepersoner = navngittePersoner; 

									if (navngittePersoner?.Any(hpr => hpr.HprNummer == sessionData.HprNumber) == true)
									{
										model.IsHprNumberBlocked = true; 
									}
								}
							}
						}
					}
				}
				
				model.IsSuccess = true; 
				model.Status = status;
				model.StatusPvk = statusPvk;
				
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
