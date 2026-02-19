using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.Models;
using System.Text;
using System.Text.Json;

namespace PatientHealthRecordsEPJ.Controllers; 

public class HelseIdController : BaseController
{
	private readonly ILogger<HomeController> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private ISession session => _httpContextAccessor.HttpContext!.Session;
	private readonly IWebHostEnvironment _webHostEnvironment;
	private readonly IOidcClientService _oidcClientService;
	private readonly IPhrApiEnvironmentProvider _phrApiEnvironmentProvider;

	SessionData sessionData;

	public HelseIdController(
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

		if (sessionData.SourcePatientInfo == null) 
		{
			return RedirectToAction(nameof(PatientController.Pasientvalg), "Patient");
		}

		await RefreshToken();

		this.sessionData = session.SessionData();

		if (!sessionData.IsAuthenticated)
			return Redirect("/");

		var model = new HelseIdViewModel()
		{
			IsSuccess = true,
			AccessToken = sessionData.AccessToken,
		};

		model.JwtToken = !string.IsNullOrWhiteSpace(sessionData.AccessToken)
			? DecodeJwtPayloadPretty(sessionData.AccessToken)
			: null;

		return View(model);
	}

	private static string DecodeJwtPayloadPretty(string jwt)
	{
		try
		{
			var parts = jwt.Split('.');
			if (parts.Length < 2)
				return "Not a JWT";

			var payload = parts[1];
			var jsonBytes = Base64UrlDecode(payload);
			var json = Encoding.UTF8.GetString(jsonBytes);

			using var doc = JsonDocument.Parse(json);
			return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex)
		{
			return $"Could not decode token: {ex.Message}";
		}
	}

	private static byte[] Base64UrlDecode(string input)
	{
		var padded = input.Replace('-', '+').Replace('_', '/');
		switch (padded.Length % 4)
		{
			case 0:
				break;
			case 2:
				padded += "==";
				break;
			case 3:
				padded += "=";
				break;
			default:
				throw new FormatException("Invalid Base64Url string length");
		}
		return Convert.FromBase64String(padded);
	}	
}
