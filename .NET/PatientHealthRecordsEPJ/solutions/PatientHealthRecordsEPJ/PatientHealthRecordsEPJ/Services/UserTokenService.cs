using NorskHelsenettUpload.Models;
using PatientHealthRecordsEPJ.Controllers;
using System.Text.Json;
using Duende.IdentityModel.Client;

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

		// Optional: check expiry first, and only refresh when needed
		if (TokenNeedsRefresh(sessionData))
		{
			await RefreshTokenAsync(httpContext);
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
		var sessionData = await GetValidAccessTokensAsync(httpContext);

		return sessionData.PjdServiceAccessToken;
	}

	public async Task<string?> GetValidPjdRepositoryAccessTokenAsync(HttpContext httpContext)
	{
		var sessionData = await GetValidAccessTokensAsync(httpContext);

		return sessionData.PjdRepositoryAccessToken;
	}

	public async Task<string?> GetValidKjernejournalAccessTokenAsync(HttpContext httpContext)
	{
		var sessionData = await GetValidAccessTokensAsync(httpContext);

		return sessionData.KjernejournalAccessToken;
	}

	public async Task RefreshTokenAsync(HttpContext httpContext)
	{
		var session = httpContext.Session;
		var sessionData = session.SessionData();

		if (sessionData == null)
			throw new InvalidOperationException("No session data found.");

		var pjdAccessTokenResult = await GetResourceAccessTokenAsync(httpContext, HomeController.PhrResource);
		var pjdServiceAccessTokenResult = await GetResourceAccessTokenAsync(httpContext, HomeController.PhrServiceResource);
		var pjdRepositoryAccessTokenResult = await GetResourceAccessTokenAsync(httpContext, HomeController.PhrRepositoryResource);
		var kjAccessTokenResult = await GetResourceAccessTokenAsync(httpContext, HomeController.KjernejournalResource);

		sessionData.PjdAccessToken = pjdAccessTokenResult?.AccessToken;
		sessionData.PjdServiceAccessToken = pjdServiceAccessTokenResult?.AccessToken;
		sessionData.PjdRepositoryAccessToken = pjdRepositoryAccessTokenResult?.AccessToken;
		sessionData.KjernejournalAccessToken = kjAccessTokenResult?.AccessToken;
		
		sessionData.SaveToSession(session);
	}

	public async Task<AccesstokenAndRefreshTokenResult?> GetResourceAccessTokenAsync(HttpContext httpContext, string resource)
	{
		var session = httpContext.Session;
		var sessionData = session.SessionData();

		if (sessionData == null)
		{
			//throw new InvalidOperationException("No session data found.");
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
		// Replace with your real expiry logic
		// For example: sessionData.ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(1)
		//return false;
		return true;
	}
}