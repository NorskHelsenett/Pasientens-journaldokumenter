using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using PatientHealthRecordsEPJ.Controllers;
using PatientHealthRecordsEPJ.Services;

namespace PatientHealthRecordsEPJ; 

public interface IOidcClientService
{
	Task InitializeAsync(string redirectUrl); // Needed if you use async setup (like discovery document fetch)
	Task<TokenResponse> RefreshTokenAsync(string refreshToken, Parameters? parameters = null); 
}

public class OidcClientService : IOidcClientService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IConfiguration _configuration;
	private readonly IHelseIdDiscoveryDocumentCache _discoveryDocumentCache;
	private bool _initialized = false;

	private DiscoveryDocumentResponse _disco;

	public OidcClientService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILoggerFactory loggerFactory, IHelseIdDiscoveryDocumentCache discoveryDocumentCache)
	{
		_httpClientFactory = httpClientFactory;
		_configuration = configuration;
		_discoveryDocumentCache = discoveryDocumentCache;
	}

	public async Task InitializeAsync(string redirectUrl)
	{
		if (_initialized) return;

		var stsUrl = HomeController.StsUrl;
		_disco = await _discoveryDocumentCache.GetDiscoveryDocumentAsync();

		_initialized = true;
	}

	public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, Parameters? parameters = null)
	{
		var httpClient = _httpClientFactory.CreateClient();
		var firstResponse = await httpClient.RequestRefreshTokenAsync(BuildRefreshTokenRequest(refreshToken, parameters));

		if (firstResponse.IsError && !string.IsNullOrWhiteSpace(firstResponse.DPoPNonce))
		{
			return await httpClient.RequestRefreshTokenAsync(BuildRefreshTokenRequest(refreshToken, parameters, firstResponse.DPoPNonce));
		}

		return firstResponse;
	}

	private RefreshTokenRequest BuildRefreshTokenRequest(string refreshToken, Parameters? parameters, string? dPoPNonce = null)
	{
		return new RefreshTokenRequest
		{
			Address = _disco.TokenEndpoint,
			ClientId = HomeController.ClientId,
			RefreshToken = refreshToken,
			GrantType = "refresh_token",
			ClientCredentialStyle = ClientCredentialStyle.PostBody,
			ClientAssertion = HomeController.GetClientAssertionPayload(_disco),
			DPoPProofToken = HelseIdDpopProofFactory.Create(Constants.JwkPrivateKey, _disco.TokenEndpoint, HttpMethod.Post.Method, dPoPNonce: dPoPNonce),
			Parameters = parameters ?? new Parameters()
		};
	}
}

