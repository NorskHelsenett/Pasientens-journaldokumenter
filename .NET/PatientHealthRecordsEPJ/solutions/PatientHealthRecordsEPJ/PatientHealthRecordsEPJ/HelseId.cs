using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PatientHealthRecordsEPJ.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PatientHealthRecordsEPJ.Services;

public sealed class HelseIdOptions
{
    public const string SectionKey = "HelseId";

    public string PrivateJwk { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
	public string Audience { get; set; } = string.Empty;
	public string Scopes { get; set; } = string.Empty;
}

public sealed class HelseIdPersontjenestenOptions
{
	public const string SectionKey = "HelseIdPersontjenesten";
	
	public string Scopes { get; set; } = string.Empty;
}

public interface IHelseIdDiscoveryDocumentCache
{
    Task<DiscoveryDocumentResponse> GetDiscoveryDocumentAsync(CancellationToken cancellationToken = default);
}

public sealed class HelseIdDiscoveryDocumentCache(IHttpClientFactory httpClientFactory) : IHelseIdDiscoveryDocumentCache
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private DiscoveryDocumentResponse? _cachedDiscoveryDocument;

    public async Task<DiscoveryDocumentResponse> GetDiscoveryDocumentAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedDiscoveryDocument is { IsError: false })
        {
            return _cachedDiscoveryDocument;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedDiscoveryDocument is { IsError: false })
            {
                return _cachedDiscoveryDocument;
            }

            var httpClient = httpClientFactory.CreateClient();
            var discoveryDocument = await httpClient.GetDiscoveryDocumentAsync(HomeController.StsUrl, cancellationToken);
            if (discoveryDocument.IsError)
            {
                throw new InvalidOperationException($"Error retrieving HelseID discovery document: {discoveryDocument.Error}");
            }

            _cachedDiscoveryDocument = discoveryDocument;
            return discoveryDocument;
        }
        finally
        {
            _lock.Release();
        }
    }
}

internal static class HelseIdClientAssertionFactory
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromSeconds(20);

    public static ClientAssertion Create(DiscoveryDocumentResponse disco, string clientId, string privateJwk)
    {
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Subject, clientId),
            new(JwtClaimTypes.IssuedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N")),
        };

        var signingCredentials = GetSigningCredentials(privateJwk);
        var header = new JwtHeader(signingCredentials)
        {
            ["typ"] = "client-authentication+jwt"
        };

        var token = new JwtSecurityToken(
            header,
            new JwtPayload(clientId, disco.Issuer, claims, DateTime.UtcNow, DateTime.UtcNow.Add(TokenLifetime)));

        return new ClientAssertion
        {
            Type = OidcConstants.ClientAssertionTypes.JwtBearer,
            Value = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }

    private static SigningCredentials GetSigningCredentials(string privateJwk)
    {
        var securityKey = new JsonWebKey(privateJwk);
        return new SigningCredentials(securityKey, SecurityAlgorithms.RsaSsaPssSha256);
    }
}

internal sealed class HelseIdClientAssertionRefreshHandler(
    DiscoveryDocumentResponse disco,
    string clientId,
    string privateJwk,
    ILogger<HelseIdClientAssertionRefreshHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method == HttpMethod.Post && IsFormUrlEncodedContent(request.Content))
        {
            await ReplaceClientAssertionAsync(request, cancellationToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task ReplaceClientAssertionAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestUri = request.RequestUri;
        if (requestUri == null || !IsHelseIdEndpoint(requestUri))
        {
            return;
        }

        var payload = await request.Content!.ReadAsStringAsync(cancellationToken);
        var formValues = ParseFormUrlEncoded(payload);
        var hasClientAssertion = formValues.Any(x => x.Key == "client_assertion");
        var hasClientAssertionType = formValues.Any(x => x.Key == "client_assertion_type");

        if (!hasClientAssertion || !hasClientAssertionType)
        {
            return;
        }

        logger.LogDebug("Refreshing client assertion for HelseID request to {Url}", requestUri);

        var refreshedAssertion = HelseIdClientAssertionFactory.Create(disco, clientId, privateJwk);
        var refreshedFormValues = new List<KeyValuePair<string, string>>(formValues.Count + 2);

        foreach (var formValue in formValues)
        {
            if (formValue.Key is "client_assertion" or "client_assertion_type")
            {
                continue;
            }

            refreshedFormValues.Add(formValue);
        }

        refreshedFormValues.Add(new KeyValuePair<string, string>("client_assertion", refreshedAssertion.Value));
        refreshedFormValues.Add(new KeyValuePair<string, string>("client_assertion_type", refreshedAssertion.Type));

        var originalHeaders = request.Content.Headers
            .Select(header => new KeyValuePair<string, IEnumerable<string>>(header.Key, header.Value.ToArray()))
            .ToList();

        request.Content = new FormUrlEncodedContent(refreshedFormValues);

        foreach (var header in originalHeaders)
        {
            request.Content.Headers.Remove(header.Key);
            request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    private bool IsHelseIdEndpoint(Uri requestUri)
    {
        if (string.IsNullOrWhiteSpace(disco.Issuer))
        {
            return false;
        }

        var issuerUri = new Uri(disco.Issuer);
        return Uri.Compare(requestUri, issuerUri, UriComponents.SchemeAndServer, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0;
    }

    private static List<KeyValuePair<string, string>> ParseFormUrlEncoded(string payload)
    {
        var values = new List<KeyValuePair<string, string>>();
        if (string.IsNullOrEmpty(payload))
        {
            return values;
        }

        foreach (var pair in payload.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = pair.IndexOf('=');
            if (separatorIndex < 0)
            {
                values.Add(new KeyValuePair<string, string>(WebUtility.UrlDecode(pair), string.Empty));
                continue;
            }

            var key = WebUtility.UrlDecode(pair[..separatorIndex]);
            var value = WebUtility.UrlDecode(pair[(separatorIndex + 1)..]);
            values.Add(new KeyValuePair<string, string>(key, value));
        }

        return values;
    }

    private static bool IsFormUrlEncodedContent(HttpContent? content)
    {
        return string.Equals(content?.Headers.ContentType?.MediaType, "application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase);
    }
}

internal static class HelseIdDpopProofFactory
{
    public static string Create(string privateJwk, string url, string httpMethod, string? dPoPNonce = null, string? accessToken = null)
    {
        var securityKey = new JsonWebKey(privateJwk);
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSsaPssSha512);

        var jwk = securityKey.Kty switch
        {
            JsonWebAlgorithmsKeyTypes.EllipticCurve => new Dictionary<string, string>
            {
                [JsonWebKeyParameterNames.Kty] = securityKey.Kty,
                [JsonWebKeyParameterNames.X] = securityKey.X,
                [JsonWebKeyParameterNames.Y] = securityKey.Y,
                [JsonWebKeyParameterNames.Crv] = securityKey.Crv,
            },
            JsonWebAlgorithmsKeyTypes.RSA => new Dictionary<string, string>
            {
                [JsonWebKeyParameterNames.Kty] = securityKey.Kty,
                [JsonWebKeyParameterNames.N] = securityKey.N,
                [JsonWebKeyParameterNames.E] = securityKey.E,
                [JsonWebKeyParameterNames.Alg] = signingCredentials.Algorithm,
            },
            _ => throw new InvalidOperationException("Invalid key type for DPoP proof.")
        };

        var jwtHeader = new JwtHeader(signingCredentials)
        {
            [JwtClaimTypes.TokenType] = "dpop+jwt",
            [JwtClaimTypes.JsonWebKey] = jwk,
        };

        var urlWithoutQuery = url.Split('?')[0];
        var payload = new JwtPayload
        {
            [JwtClaimTypes.JwtId] = Guid.NewGuid().ToString(),
            [JwtClaimTypes.DPoPHttpMethod] = httpMethod,
            [JwtClaimTypes.DPoPHttpUrl] = urlWithoutQuery,
            [JwtClaimTypes.IssuedAt] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };

        if (!string.IsNullOrEmpty(dPoPNonce))
        {
            payload[JwtClaimTypes.Nonce] = dPoPNonce;
        }

        if (!string.IsNullOrEmpty(accessToken))
        {
            var hash = SHA256.HashData(Encoding.ASCII.GetBytes(accessToken));
            payload[JwtClaimTypes.DPoPAccessTokenHash] = Base64UrlEncoder.Encode(hash);
        }

        var jwtSecurityToken = new JwtSecurityToken(jwtHeader, payload);
        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }
}

public static class HttpClientExtensions
{
    public static IHttpClientBuilder UseHelseIdDPoP(this IHttpClientBuilder httpClientBuilder, Action<HelseIdOptions>? configureClient = null)
    {
        httpClientBuilder.AddHttpMessageHandler(serviceProvider =>
        {
			var baseOptions = serviceProvider.GetRequiredService<IOptions<HelseIdOptions>>().Value;

			// Clone so that each client gets its own independent copy
			var clientOptions = new HelseIdOptions
			{
				PrivateJwk = baseOptions.PrivateJwk,
				ClientId = baseOptions.ClientId,
				TokenEndpoint = baseOptions.TokenEndpoint,
				Audience = baseOptions.Audience,
				Scopes = baseOptions.Scopes
			};

			// Apply per-client overrides
			configureClient?.Invoke(clientOptions);

			// Validate
			ArgumentException.ThrowIfNullOrWhiteSpace(clientOptions.PrivateJwk);
			ArgumentException.ThrowIfNullOrWhiteSpace(clientOptions.ClientId);
			ArgumentException.ThrowIfNullOrWhiteSpace(clientOptions.TokenEndpoint);
			ArgumentException.ThrowIfNullOrWhiteSpace(clientOptions.Scopes);

			// Construct handler dependencies using cloned options
			var opt = Options.Create(clientOptions);
			var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
			var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
			var dPoPProofCreator = new DPoPProofCreator(opt, loggerFactory.CreateLogger<DPoPProofCreator>());
			var tokenService = new TokenService(opt, loggerFactory.CreateLogger<TokenService>(), httpClientFactory, dPoPProofCreator);

			return new HelseIdDelegatingHandler(dPoPProofCreator, tokenService);
        });

#if DEBUG
        httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
		{
			return new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback =
					HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
			};
		});
#endif

        return httpClientBuilder;
    }
}


internal class HelseIdDelegatingHandler(DPoPProofCreator dPoPProofCreator, TokenService tokenService)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await tokenService.GetAccessToken(cancellationToken);
        if (request.RequestUri != null)
        {
            var proof = dPoPProofCreator.CreateDPoPProof(
                request.RequestUri.ToString(),
                request.Method.ToString(),
                null,
                accessToken);
            request.SetDPoPToken(accessToken, proof);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}


public class DPoPProofCreator(IOptions<HelseIdOptions> options, ILogger<DPoPProofCreator> logger)
{
    public string CreateDPoPProof(string url, string httpMethod, string? dPoPNonce = null, string? accessToken = null)
    {
        logger.LogDebug("Creating DPoP proof for url {Url} with nonce: {Nonce} ", url, dPoPNonce);

        return HelseIdDpopProofFactory.Create(options.Value.PrivateJwk, url, httpMethod, dPoPNonce, accessToken);
    }
}


public class TokenService(
    IOptions<HelseIdOptions> options,
    ILogger<TokenService> logger,
    IHttpClientFactory httpClientFactory,
    DPoPProofCreator dPoPProofCreator)
{
    private static readonly TimeSpan ClientAssertionLifetime = TimeSpan.FromSeconds(20);

    private DateTime _cachedAccessTokenExpiresAt = DateTime.MinValue;
    private string _cachedAccessToken = string.Empty;
    private HttpClient? _httpClient;

    private HttpClient HttpClient => _httpClient ??= httpClientFactory.CreateClient("HelseId.TokenService");

    public async Task<string> GetAccessToken(CancellationToken cancellationToken)
    {
        if (DateTime.UtcNow <= _cachedAccessTokenExpiresAt)
        {
            logger.LogDebug("Using cached DPoP access token");
            return _cachedAccessToken;
        }

        logger.LogDebug("Getting DPoP access token from HelseId");
        var tokenResponse = await GetAccessTokenFromHelseId(cancellationToken);

        _cachedAccessTokenExpiresAt =
            DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 30); // Refresh token 30 seconds ahead of expiry
        _cachedAccessToken = tokenResponse.AccessToken!;

        return _cachedAccessToken;
    }

    private async Task<TokenResponse> GetAccessTokenFromHelseId(CancellationToken cancellationToken)
    {
        // 1. Send a token request without a DPoP nonce
        var firstRequest = CreateTokenRequest();
        var firstTokenResponse = await HttpClient.RequestClientCredentialsTokenAsync(firstRequest, cancellationToken);
        if (!firstTokenResponse.IsError || string.IsNullOrEmpty(firstTokenResponse.DPoPNonce))
        {
            throw new Exception("Expected a DPoP nonce to be returned from the authorization server.");
        }

        // 2. Send a second token request with the DPoP nonce from the first response
        var secondRequest = CreateTokenRequest(firstTokenResponse.DPoPNonce);
        var secondTokenResponse = await HttpClient.RequestClientCredentialsTokenAsync(secondRequest, cancellationToken);
        if (secondTokenResponse.IsError || secondTokenResponse.AccessToken == null)
        {
            throw new Exception($"Error retrieving access token: {secondTokenResponse.Error}");
        }

        return secondTokenResponse;
    }

    private ClientCredentialsTokenRequest CreateTokenRequest(string? dPoPNonce = null)
    {
        var securityKey = new JsonWebKey(options.Value.PrivateJwk);
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSsaPssSha512);

		var header = new JwtHeader(signingCredentials)
		{
			["typ"] = "client-authentication+jwt"
		};

        // Tell HelseID which org(s) you act for        
        var parentOrg = HomeController.ParentOrg; 
		string? childOrg = null;            // optional child org

		var helseidAuthorization = new[]
		{
		    new Dictionary<string, object>
		    {
			    ["type"] = "helseid_authorization",
			    ["practitioner_role"] = new Dictionary<string, object>
			    {
				    ["organization"] = new Dictionary<string, object>
				    {
					    ["identifier"] = new Dictionary<string, object>
					    {
						    ["system"] = "urn:oid:1.0.6523",
						    ["type"]   = "ENH",
						    ["value"]  = string.IsNullOrWhiteSpace(childOrg)
							    ? $"NO:ORGNR:{parentOrg}"
							    : $"NO:ORGNR:{parentOrg}:{childOrg}"
					    }
				    }
			    }
		    }
	    };

		var claims = new List<Claim>
		{
			new(JwtClaimTypes.Subject, options.Value.ClientId),
			new(JwtClaimTypes.IssuedAt, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
				ClaimValueTypes.Integer64),
			new(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N")),
            //new Claim("helseid://claims/client/claims/orgnr_parent", "999977774")
			new Claim("helseid://claims/client/claims/orgnr_parent", "930161012"),
			new("assertion_details", JsonSerializer.Serialize(helseidAuthorization), JsonClaimValueTypes.JsonArray)
		};

		var tokenEndpointUri = new Uri(options.Value.TokenEndpoint);
		var audience = $"{tokenEndpointUri.Scheme}://{tokenEndpointUri.Host}";

		var payload = new JwtPayload(
			issuer: options.Value.ClientId,
			//audience: options.Value.TokenEndpoint,
			audience: audience,
			claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(ClientAssertionLifetime)
		);

		var token = new JwtSecurityToken(header, payload);

		var tokenHandler = new JwtSecurityTokenHandler();
        var clientAssertion = tokenHandler.WriteToken(token);

        var request = new ClientCredentialsTokenRequest
        {
            Address = options.Value.TokenEndpoint,            
            ClientAssertion =
                new ClientAssertion
                {
                    Value = clientAssertion,
                    Type = OidcConstants.ClientAssertionTypes.JwtBearer
                },            
            ClientId = options.Value.ClientId,
            Scope = options.Value.Scopes,
            GrantType = OidcConstants.GrantTypes.ClientCredentials,
            ClientCredentialStyle = ClientCredentialStyle.PostBody,
            DPoPProofToken =
                dPoPProofCreator.CreateDPoPProof(options.Value.TokenEndpoint, "POST", dPoPNonce: dPoPNonce)
        };

        return request;
    }
}
