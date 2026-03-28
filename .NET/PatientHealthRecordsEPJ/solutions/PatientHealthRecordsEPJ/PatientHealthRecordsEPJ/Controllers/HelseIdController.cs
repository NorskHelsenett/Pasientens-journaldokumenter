using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.Models;
using PatientHealthRecordsEPJ.Services;
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

		var model = new HelseIdViewModel()
		{
			IsSuccess = true,
			AccessToken = sessionData.PjdAccessToken,
		};

		model.JwtToken = !string.IsNullOrWhiteSpace(sessionData.PjdAccessToken)
			? DecodeJwtPayloadPretty(sessionData.PjdAccessToken)
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
