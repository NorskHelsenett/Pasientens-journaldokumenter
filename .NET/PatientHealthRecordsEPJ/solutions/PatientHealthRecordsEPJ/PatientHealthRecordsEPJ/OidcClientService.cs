using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.DPoP;
using Duende.IdentityModel.OidcClient.Results;
using PatientHealthRecordsEPJ.Controllers;
using PatientHealthRecordsEPJ.Services;

namespace PatientHealthRecordsEPJ; 

public interface IOidcClientService
{
	OidcClient Client { get; }
	Task InitializeAsync(string redirectUrl); // Needed if you use async setup (like discovery document fetch)
	Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, Parameters? parameters = null); 
}

public class OidcClientService : IOidcClientService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IConfiguration _configuration;
	private readonly ILoggerFactory _loggerFactory;
	private OidcClient _oidcClient;
	private bool _initialized = false;

	private DiscoveryDocumentResponse _disco;

	public OidcClient Client => _oidcClient;

	public OidcClientService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILoggerFactory loggerFactory)
	{
		_httpClientFactory = httpClientFactory;
		_configuration = configuration;
		_loggerFactory = loggerFactory;
	}

	public async Task InitializeAsync(string redirectUrl)
	{
		if (_initialized) return;

		var httpClient = _httpClientFactory.CreateClient();
		//var stsUrl = _configuration["HelseId:StsUrl"]; // Put config in appsettings.json
		//var redirectUrl = _configuration["HelseId:RedirectUrl"];

		var stsUrl = HomeController.StsUrl;

		//var disco = await httpClient.GetDiscoveryDocumentAsync(stsUrl);
		_disco = await httpClient.GetDiscoveryDocumentAsync(stsUrl);

		var options = new OidcClientOptions
		{
			Authority = stsUrl,
			//ClientId = _configuration["HelseId:ClientId"],
			ClientId = HomeController.ClientId,
			RedirectUri = redirectUrl,
			LoadProfile = false,
			//IdentityTokenValidator = new JwtHandlerIdentityTokenValidator(),
			BackchannelHandler = new HelseIdClientAssertionRefreshHandler(
				_disco,
				HomeController.ClientId,
				Constants.JwkPrivateKey,
				_loggerFactory.CreateLogger<HelseIdClientAssertionRefreshHandler>())
			{
				InnerHandler = new HttpClientHandler()
			},
		};

		options.ConfigureDPoP(Constants.JwkPrivateKey);
		_oidcClient = new OidcClient(options);

		_initialized = true;
	}

	public async Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, Parameters? parameters = null)
	{
		_oidcClient.Options.ClientAssertion = HomeController.GetClientAssertionPayload(_disco);
		return await _oidcClient.RefreshTokenAsync(refreshToken, parameters);
	}
}

