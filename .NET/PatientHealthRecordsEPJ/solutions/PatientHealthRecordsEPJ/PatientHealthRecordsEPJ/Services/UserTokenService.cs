using NorskHelsenettUpload.Models;
using PatientHealthRecordsEPJ.Controllers;
using System.Text.Json;
using Duende.IdentityModel.Client;
using System.IdentityModel.Tokens.Jwt;

namespace PatientHealthRecordsEPJ.Services;

public interface IUserTokenService
{
	Task<SessionData> GetValidAccessTokensAsync(HttpContext httpContext);
	Task<string?> GetValidPjdAccessTokenAsync(HttpContext httpContext);
	Task<string?> GetValidPjdServiceAccessTokenAsync(HttpContext httpContext);
	Task<string?> GetValidPjdRepositoryAccessTokenAsync(HttpContext httpContext);
	Task<string?> GetValidKjernejournalAccessTokenAsync(HttpContext httpContext); 
	Task RefreshTokenAsync(HttpContext httpContext);
}

public class UserTokenService : IUserTokenService
{

	public class AccesstokenAndRefreshTokenResult
	{
		public bool Success { get; set; }
		public string AccessToken { get; set; } = default!;
		public string RefreshToken { get; set; } = default!;
		public string? Error { get; set; } = default!;
		public string? ErrorDescription { get; set; } = default!;
	}

	private readonly IOidcClientService _oidcClientService;

	public UserTokenService(IOidcClientService oidcClientService)
	{
		_oidcClientService = oidcClientService;
	}

	public async Task<SessionData> GetValidAccessTokensAsync(HttpContext httpContext)
	{
		var session = httpContext.Session;
		var sessionData = session.SessionData();

		if (sessionData == null)
			throw new InvalidOperationException("No session data found.");

		if (TokenNeedsRefresh(sessionData))
		{
			await EnsureResourceAccessTokenAsync(httpContext, HomeController.PhrResource);
			sessionData = session.SessionData();
		}

		return sessionData;
	}

	public async Task<string?> GetValidPjdAccessTokenAsync(HttpContext httpContext)
	{
		var sessionData = await GetValidAccessTokensAsync(httpContext);

		return sessionData.PjdAccessToken;
	}

	public async Task<string?> GetValidPjdServiceAccessTokenAsync(HttpContext httpContext)
	{
		var sessionData = await EnsureResourceAccessTokenAsync(httpContext, HomeController.PhrServiceResource);

		return sessionData.PjdServiceAccessToken;
	}

	public async Task<string?> GetValidPjdRepositoryAccessTokenAsync(HttpContext httpContext)
	{
		var sessionData = await EnsureResourceAccessTokenAsync(httpContext, HomeController.PhrRepositoryResource);

		return sessionData.PjdRepositoryAccessToken;
	}

	public async Task<string?> GetValidKjernejournalAccessTokenAsync(HttpContext httpContext)
	{
		var sessionData = await EnsureResourceAccessTokenAsync(httpContext, HomeController.KjernejournalResource);

		return sessionData.KjernejournalAccessToken;
	}

	public async Task RefreshTokenAsync(HttpContext httpContext)
	{
		var session = httpContext.Session;
		var sessionData = session.SessionData();

		if (sessionData == null)
			throw new InvalidOperationException("No session data found.");

		var pjdAccessTokenResult = await RefreshResourceAccessTokenAsync(httpContext, HomeController.PhrResource, sessionData);
		var pjdServiceAccessTokenResult = await RefreshResourceAccessTokenAsync(httpContext, HomeController.PhrServiceResource, sessionData);
		var pjdRepositoryAccessTokenResult = await RefreshResourceAccessTokenAsync(httpContext, HomeController.PhrRepositoryResource, sessionData);
		var kjAccessTokenResult = await RefreshResourceAccessTokenAsync(httpContext, HomeController.KjernejournalResource, sessionData);

		ApplyTokenResult(sessionData, HomeController.PhrResource, pjdAccessTokenResult);
		ApplyTokenResult(sessionData, HomeController.PhrServiceResource, pjdServiceAccessTokenResult);
		ApplyTokenResult(sessionData, HomeController.PhrRepositoryResource, pjdRepositoryAccessTokenResult);
		ApplyTokenResult(sessionData, HomeController.KjernejournalResource, kjAccessTokenResult);
		
		sessionData.SaveToSession(session);
	}

	public async Task<AccesstokenAndRefreshTokenResult?> GetResourceAccessTokenAsync(HttpContext httpContext, string resource)
	{
		return await RefreshResourceAccessTokenAsync(httpContext, resource, httpContext.Session.SessionData());
	}

	private async Task<SessionData> EnsureResourceAccessTokenAsync(HttpContext httpContext, string resource)
	{
		var session = httpContext.Session;
		var sessionData = session.SessionData();

		if (sessionData == null)
		{
			throw new InvalidOperationException("No session data found.");
		}

		if (sessionData.RefreshToken == null)
		{
			return sessionData;
		}

		var existingToken = GetResourceAccessToken(sessionData, resource);
		var existingTokenExpiresAt = GetResourceAccessTokenExpiresAt(sessionData, resource);

		if (!string.IsNullOrWhiteSpace(existingToken) && !TokenNeedsRefresh(existingTokenExpiresAt))
		{
			return sessionData;
		}

		var tokenResult = await RefreshResourceAccessTokenAsync(httpContext, resource, sessionData);
		ApplyTokenResult(sessionData, resource, tokenResult);
		sessionData.SaveToSession(session);

		return sessionData;
	}

	private async Task<AccesstokenAndRefreshTokenResult?> RefreshResourceAccessTokenAsync(HttpContext httpContext, string resource, SessionData? sessionData)
	{
		if (sessionData == null)
		{
			return new AccesstokenAndRefreshTokenResult()
			{
				Success = false,
				Error = "NoSessionData",
				ErrorDescription = "SessionData is null"
			};
		}

		if (sessionData.RefreshToken == null)
		{
			return new AccesstokenAndRefreshTokenResult()
			{
				Success = false,
				Error = "MissingRefreshToken",
				ErrorDescription = "Refreshtoken is null"
			};
		}

		var redirectUrl = GetRedirectUrl(httpContext);

		await _oidcClientService.InitializeAsync(redirectUrl);

		var parameters = new Parameters
		{
			{ "resource", resource }
		};

		var result = await _oidcClientService.RefreshTokenAsync(sessionData.RefreshToken, parameters);

		if (result.IsError)
		{			
			return new AccesstokenAndRefreshTokenResult() 
			{ 
				Error = result.Error, 
				ErrorDescription = result.ErrorDescription 
			};
		}

		var tokenResult = new AccesstokenAndRefreshTokenResult()
		{
			Success = true,
			AccessToken = result.AccessToken,
			RefreshToken = result.RefreshToken,
		};

		return tokenResult;
	}

	private static string GetRedirectUrl(HttpContext httpContext)
	{
#if DEBUG
		return $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}/callback";
#else
        return HomeController.TestRedirectUrl;
#endif
	}

	private static bool TokenNeedsRefresh(SessionData sessionData)
	{
		return string.IsNullOrWhiteSpace(sessionData.PjdAccessToken) || TokenNeedsRefresh(sessionData.PjdAccessTokenExpiresAt);
	}

	private static bool TokenNeedsRefresh(DateTimeOffset? expiresAt)
	{
		return !expiresAt.HasValue || expiresAt.Value <= DateTimeOffset.UtcNow.AddMinutes(1);
	}

	private static void ApplyTokenResult(SessionData sessionData, string resource, AccesstokenAndRefreshTokenResult? tokenResult)
	{
		if (tokenResult?.Success != true || string.IsNullOrWhiteSpace(tokenResult.AccessToken))
		{
			return;
		}

		var expiresAt = GetTokenExpiration(tokenResult.AccessToken);

		switch (resource)
		{
			case var _ when resource == HomeController.PhrResource:
				sessionData.PjdAccessToken = tokenResult.AccessToken;
				sessionData.PjdAccessTokenExpiresAt = expiresAt;
				break;
			case var _ when resource == HomeController.PhrServiceResource:
				sessionData.PjdServiceAccessToken = tokenResult.AccessToken;
				sessionData.PjdServiceAccessTokenExpiresAt = expiresAt;
				break;
			case var _ when resource == HomeController.PhrRepositoryResource:
				sessionData.PjdRepositoryAccessToken = tokenResult.AccessToken;
				sessionData.PjdRepositoryAccessTokenExpiresAt = expiresAt;
				break;
			case var _ when resource == HomeController.KjernejournalResource:
				sessionData.KjernejournalAccessToken = tokenResult.AccessToken;
				sessionData.KjernejournalAccessTokenExpiresAt = expiresAt;
				break;
		}

		if (!string.IsNullOrWhiteSpace(tokenResult.RefreshToken))
		{
			sessionData.RefreshToken = tokenResult.RefreshToken;
		}
	}

	private static string? GetResourceAccessToken(SessionData sessionData, string resource)
	{
		return resource switch
		{
			var _ when resource == HomeController.PhrResource => sessionData.PjdAccessToken,
			var _ when resource == HomeController.PhrServiceResource => sessionData.PjdServiceAccessToken,
			var _ when resource == HomeController.PhrRepositoryResource => sessionData.PjdRepositoryAccessToken,
			var _ when resource == HomeController.KjernejournalResource => sessionData.KjernejournalAccessToken,
			_ => null
		};
	}

	private static DateTimeOffset? GetResourceAccessTokenExpiresAt(SessionData sessionData, string resource)
	{
		return resource switch
		{
			var _ when resource == HomeController.PhrResource => sessionData.PjdAccessTokenExpiresAt,
			var _ when resource == HomeController.PhrServiceResource => sessionData.PjdServiceAccessTokenExpiresAt,
			var _ when resource == HomeController.PhrRepositoryResource => sessionData.PjdRepositoryAccessTokenExpiresAt,
			var _ when resource == HomeController.KjernejournalResource => sessionData.KjernejournalAccessTokenExpiresAt,
			_ => null
		};
	}

	private static DateTimeOffset? GetTokenExpiration(string accessToken)
	{
		var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
		var expClaim = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp)?.Value;

		if (!long.TryParse(expClaim, out var exp))
		{
			return null;
		}

		return DateTimeOffset.FromUnixTimeSeconds(exp);
	}
}