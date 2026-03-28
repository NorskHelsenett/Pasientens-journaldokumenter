using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.Mapper.Xml;
using PatientHealthRecordsEPJ.Models;
using PatientHealthRecordsEPJ.Models.HelsenorgeLogin;
using PatientHealthRecordsEPJ.Personverninnstillinger;
using PatientHealthRecordsEPJ.Services;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using XcaXds.Commons.Models.ClinicalDocument.Types;

namespace PatientHealthRecordsEPJ.Controllers; 

public class KjernejournalLoginController : BaseController
{
	private readonly ILogger<HomeController> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private ISession session => _httpContextAccessor.HttpContext!.Session;
	private readonly IWebHostEnvironment _webHostEnvironment;
	private readonly IOidcClientService _oidcClientService;
	private readonly IPhrApiEnvironmentProvider _phrApiEnvironmentProvider;
	private readonly KjernejournalLoginApiService _kjernejournalLoginApiService;

	SessionData sessionData;

	public KjernejournalLoginController(
		IOptions<SessionOptions> sessionOptions, 
		ILogger<HomeController> logger, 
		IHttpContextAccessor httpContextAccessor, 
		IMemoryCache memoryCache, 
		IWebHostEnvironment webHostEnvironment,		
		IOidcClientService oidcClientService,
		IPhrApiEnvironmentProvider phrApiEnvironmentProvider,
		KjernejournalLoginApiService kjernejournalLoginApiService,
		IUserTokenService userTokenService
		) : base(sessionOptions, userTokenService)
	{
		_logger = logger;
		_httpContextAccessor = httpContextAccessor;
		_webHostEnvironment = webHostEnvironment;
		_oidcClientService = oidcClientService;
		_phrApiEnvironmentProvider = phrApiEnvironmentProvider;
		_kjernejournalLoginApiService = kjernejournalLoginApiService;
	}
	
	public async Task<IActionResult> Index()
	{
		var sessionData = await _userTokenService.GetValidAccessTokensAsync(HttpContext);

		if (sessionData.KjernejournalAccessToken == null)
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

		//var model = await GetStatus(sessionData.PatientId!);

		//return View(model);
		return View(); 
	}

	public async Task<IActionResult> KjLogin(string patient)	
	{ 
		var sessionData = session.SessionData();

		var model = await Login(patient);

		return View("Login", model);
	}

	public async Task<IActionResult> Test(string patient)
	{
		var sessionData = session.SessionData();

		var model = await Test2(patient);

		return View("Login", model);
	}

	private async Task<KjernejournalLoginViewModel> Login(
		string patient
		)
	{
		var sessionData = session.SessionData();

		KjernejournalLoginViewModel model = new KjernejournalLoginViewModel();

		var kjernejournalSession = await _kjernejournalLoginApiService.CreateSession(sessionData);
		sessionData.SaveToSession(session);

		model.KjernejournalSession = kjernejournalSession;
		
		model.IsSuccess = true; 

		var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

		/*var apiUrl = _apiEnvironment.StatusUrl;

		var jsonContent = JsonSerializer.Serialize(new { nin = patient });

		var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"); 

		var apiResult = await ApiCaller.CallApi(HttpMethod.Post, apiUrl, sessionData.AccessToken!, document: false, headerKeyValues: headerKeyValues, content: content, isReadBodyAsString: true);

		*/

		return model; 
	}

	private async Task<KjernejournalLoginViewModel> Test2(
		string patient
		)
	{
		var sessionData = session.SessionData();

		KjernejournalLoginViewModel model = new KjernejournalLoginViewModel();

		var kjernejournalSession = await _kjernejournalLoginApiService.CreateSession(sessionData);
		sessionData.SaveToSession(session);

		var kjLoginUrl = $"https://st1.kjernejournal-test.no/hpp-webapp/hentpasient.html?code={kjernejournalSession?.code}&ehr_code_verifier={sessionData.KjernejournalEhrCodeVerifier}";

		var httpClient = new HttpClient();

		var response = await httpClient.GetAsync(kjLoginUrl);

		var responseBody = await response.Content.ReadAsStringAsync();

		// Read response cookies
		var cookies = response.Headers.GetValues("Set-Cookie");

		// Read into cookie container
		CookieContainer cookiesContainer = new CookieContainer();
		foreach (var cookie in cookies)
		{
			cookiesContainer.SetCookies(new Uri("https://st1.kjernejournal-test.no"), cookie);
		}

		// Make a request to https://st1.kjernejournal-test.no/hpp-webapp/api/journaldokumenter/dokumentliste and set the cookie
		httpClient.DefaultRequestHeaders.Add("Cookie", cookiesContainer.GetCookieHeader(new Uri("https://st1.kjernejournal-test.no/hpp-webapp")));
		httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

		try
		{
			response = await httpClient.GetAsync("https://st1.kjernejournal-test.no/hpp-webapp/api/journaldokumenter/dokumentliste");

			responseBody = await response.Content.ReadAsStringAsync();
		}
		catch (Exception ex)
		{
		}

		// https://st1.kjernejournal-test.no/hpp-webapp/api/journaldokumenter/personvernsinnstillinger
		// https://st1.kjernejournal-test.no/hpp-webapp/api/journaldokumenter/metadata
		// https://st1.kjernejournal-test.no/hpp-webapp/api/journaldokumenter/dokumentliste
		// JSESSIONID=c_PI5pZErKgYAyoUCSzXFjP2Zt6NWTTlJxTS922ksDiwHzDkcjrW!599544875!634396076

		model.KjernejournalSession = kjernejournalSession;

		model.IsSuccess = true;

		var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

		/*var apiUrl = _apiEnvironment.StatusUrl;

		var jsonContent = JsonSerializer.Serialize(new { nin = patient });

		var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"); 

		var apiResult = await ApiCaller.CallApi(HttpMethod.Post, apiUrl, sessionData.AccessToken!, document: false, headerKeyValues: headerKeyValues, content: content, isReadBodyAsString: true);

		*/

		return model;
	}

	public async Task<KjernejournalLoginViewModel> Helsenorge(
		string patient
		)
	{
		var sessionData = session.SessionData();

		//KjernejournalLoginViewModel model = new KjernejournalLoginViewModel();

		//var kjernejournalSession = await _kjernejournalLoginApiService.CreateSession(sessionData);
		//sessionData.SaveToSession(session);

		var hnLoginUrl = $"https://tjenester.hn.test.nhn.no/?pnr={sessionData.PatientId}";
		var hnDocumentListUrl = $"https://tjenester.hn.test.nhn.no/proxy/Pasientjournal/api/v1/Xca/Liste?FraDato=null&TilDato=null&Kategori=null&Init=true";

		var httpClient = new HttpClient();

		httpClient.DefaultRequestHeaders.Add("Host", "tjenester.hn.test.nhn.no");
		httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");
		httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36 Edg/145.0.0.0");

		//sec - ch - ua: "Not:A-Brand"; v = "99", "Microsoft Edge"; v = "145", "Chromium"; v = "145"
		//sec - ch - ua - mobile: ?0
		//sec - ch - ua - platform: "Windows"


		var response = await httpClient.GetAsync(hnLoginUrl);

		// Uses transfer encoding chunked, make sure we've read the whole response with cookies

		var responseBody = await response.Content.ReadAsStringAsync();
	

		//var responseBody = await response.Content.ReadAsStringAsync();

		// Read response cookies
		var cookies = response.Headers.GetValues("Set-Cookie");

		// Read into cookie container
		CookieContainer cookiesContainer = new CookieContainer();
		foreach (var cookie in cookies)
		{
			//cookiesContainer.SetCookies(new Uri("https://tjenester.hn.test.nhn.no"), cookie);
			//cookiesContainer.SetCookies(new Uri(cookie), cookie);

			// Add to cookie container with domain
			var cookieParts = cookie.Split(';', 2);
			var cookieNameValue = cookieParts[0].Split('=', 2);
			var cookieDomain = cookieParts.Length > 1 ? cookieParts[1].Trim() : "tjenester.hn.test.nhn.no";
			cookiesContainer.Add(new Cookie(cookieNameValue[0], cookieNameValue[1], "/", cookieDomain));
		}

		// Read the values of all these cookies: 
		//Set - Cookie: MH_Idp_test1 = idportenlevel4; expires = Sat, 14 Mar 2026 20:01:45 GMT; max - age = 259199; domain =.hn.test.nhn.no; path =/; secure
		//Set - Cookie: MH_AccessToken_test1 = eyJhbGciOiJIUzI1NiIsInZlciI6MSwidHlwXzIiOiJyZWYiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJzaWtrZXJoZXQtaW50ZXJuLmhlbHNlbm9yZ2Uubm8iLCJleHAiOjE3NzMyODgxMDUsImp0aSI6IjZlOGNmNWY0LTA3YTItNGVkYi05ZGU2LTBjNGI2OGQ1NzQ3ZSIsImNsaWVudF9pZCI6IjcxZTFkZmYxLTJiM2EtNGMzNy05NGUyLWNlY2E0MmUyOGJhYyIsImNsaWVudF9uYW1lIjoiVGplbmVzdGVyLldlYkFwcCIsInJlZl90b2tlbiI6IkFVc3VyRHpDZ3VpU1QtXzhjTDlRY0NRY3NmZlJsUGJqY1pGbjRnNThMRW8iLCJyZWZfdG9rZW5fdHlwZSI6IkZ1bGwiLCJ6b25lIjoiaW50ZXJuIiwiZW5kdXNlcnR5cGUiOiJjaXRpemVuIiwiYXVkIjoiNzFlMWRmZjEtMmIzYS00YzM3LTk0ZTItY2VjYTQyZTI4YmFjIiwiaWF0IjoxNzczMjU5MzA1LCJuYmYiOjE3NzMyNTkzMDV9.rmcZOTILlf9SgN5nZOAOEyCO_u0TOq4OYvKoihZiHHk; domain = tjenester.hn.test.nhn.no; path =/; secure; httponly
		//Set - Cookie: MH_LoggedIn_test1 = 1; domain =.hn.test.nhn.no; path =/; secure
		//Set - Cookie: HN_CSRF_Token_test1 = eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCIsInZlciI6MX0.eyJ0b2tlbiI6IktGT1E5ZjhKR1poVVNKVVZySTRJM0tmXzF6LVFPeU5zZ2VRMkd5dkxTbXI2WnFBcVlsTkU1ZTVZMWJUdU11S0szcjAiLCJ0b2tlbl9oYXNoIjoieU5uRDU4c1VvNGkzTzZ3elEyUjdNLW0xSlZabmlaWHpwb2tLOEZpSTE2byIsIm5iZiI6MTc3MzI1OTMwNSwiZXhwIjoxNzczMjkxNzA1LCJpYXQiOjE3NzMyNTkzMDUsImlzcyI6ImhlbHNlbm9yZ2Uubm8ifQ.U49gsk49LQBfU2bTeAl6GnNpHRpxYuehzLH6CeAzjFw; domain =.hn.test.nhn.no; path =/; secure
		//Set - Cookie: hn - language = nb - NO; expires = Thu, 12 Mar 2026 20:01:45 GMT; max - age = 86399; domain =.hn.test.nhn.no; path =/; secure

		var mhIdp = cookiesContainer.GetCookies(new Uri("https://tjenester.hn.test.nhn.no"))["MH_Idp_test1"]?.Value;
		var mhAccessToken = cookiesContainer.GetCookies(new Uri("https://tjenester.hn.test.nhn.no"))["MH_AccessToken_test1"]?.Value;
		var mhLoggedIn = cookiesContainer.GetCookies(new Uri("https://tjenester.hn.test.nhn.no"))["MH_LoggedIn_test1"]?.Value;
		//var mhLoggedIn2 = cookiesContainer.GetCookies(new Uri("https://hn.test.nhn.no"))["MH_LoggedIn_test1"]?.Value;
		var hnLanguage = cookiesContainer.GetCookies(new Uri("https://tjenester.hn.test.nhn.no"))["hn-language"]?.Value;
		var csrfToken = cookiesContainer.GetCookies(new Uri("https://tjenester.hn.test.nhn.no"))["HN_CSRF_Token_test1"]?.Value;
		
		//var csrfToken = cookiesContainer.GetCookies(new Uri("https://tjenester.hn.test.nhn.no"))["HN_CSRF_Token_test1"]?.Value;


		HelsenorgeDocumentList documentList = new HelsenorgeDocumentList();

		// Make a request to https://tjenester.hn.test.nhn.no/hpp-webapp/api/journaldokumenter/dokumentliste and set the cookie

		//httpClient.DefaultRequestHeaders.Add("Cookie", cookiesContainer.GetCookieHeader(new Uri("https://tjenester.hn.test.nhn.no/hpp-webapp")));
		// Set all cookies from cookie container in httpClient.DefaultRequestHeaders
		var cookieHeader = cookiesContainer.GetCookieHeader(new Uri("https://tjenester.hn.test.nhn.no"));
		httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);

		httpClient.DefaultRequestHeaders.Add("Accept", "application/json");


		// as x-hn-csrf-token use the value of the cookie named "HN_CSRF_Token_test1"! from the response of the login request		
		httpClient.DefaultRequestHeaders.Add("x-hn-csrf-token", csrfToken);

		httpClient.DefaultRequestHeaders.Add("x-hn-hendelselogg", "Innsyn pasientjournal");

		try
		{
			response = await httpClient.GetAsync(hnDocumentListUrl);

			responseBody = await response.Content.ReadAsStringAsync();

			documentList = JsonSerializer.Deserialize<HelsenorgeDocumentList>(responseBody, new JsonSerializerOptions
			{
				//PropertyNamingPolicy = JsonNamingPolicy.,
				PropertyNameCaseInsensitive = true
			});
		}
		catch (Exception ex)
		{
		}

		// https://st1.kjernejournal-test.no/hpp-webapp/api/journaldokumenter/personvernsinnstillinger
		// https://st1.kjernejournal-test.no/hpp-webapp/api/journaldokumenter/metadata
		// https://st1.kjernejournal-test.no/hpp-webapp/api/journaldokumenter/dokumentliste
		// JSESSIONID=c_PI5pZErKgYAyoUCSzXFjP2Zt6NWTTlJxTS922ksDiwHzDkcjrW!599544875!634396076

		//model.KjernejournalSession = kjernejournalSession;

		//model.IsSuccess = true;

		var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

		/*var apiUrl = _apiEnvironment.StatusUrl;

		var jsonContent = JsonSerializer.Serialize(new { nin = patient });

		var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"); 

		var apiResult = await ApiCaller.CallApi(HttpMethod.Post, apiUrl, sessionData.AccessToken!, document: false, headerKeyValues: headerKeyValues, content: content, isReadBodyAsString: true);

		*/

		//return model;
		return null; 
	}
}
