using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.DPoP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using NorskHelsenettUpload.Models;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.Models;
using PatientHealthRecordsEPJ.Services;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PatientHealthRecordsEPJ.Controllers; 

public class HomeController : BaseController
{
	private static readonly TimeSpan HelseIdJwtLifetime = TimeSpan.FromSeconds(20);

	private readonly ILogger<HomeController> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private ISession session => _httpContextAccessor.HttpContext!.Session;
	private readonly IMemoryCache _memoryCache;
	private readonly IOidcClientService _oidcClientService;
	private readonly OrganizationService _organizationService;
	private readonly PhrApiOptions _phrApiOptions;
	private readonly IPhrApiEnvironmentProvider _phrApiEnvironmentProvider; 

	public const string ClientId = "2141d734-7604-42ca-a377-27f1b712f4e6"; // "Patient Health Records API EPJ". Privatnøkkel ligger i Constants.cs

	// The client is configured in the HelseID test environment, so we will point to that
	public const string StsUrl = "https://helseid-sts.test.nhn.no";

	// Define audience of all APIs we want to call. We'll use resource indicators to get tokens scoped only to each API. 
	public const string PhrResource = "nhn:phr";
	public const string PhrServiceResource = "nhn:phr-svc";
	public const string PhrRepositoryResource = "nhn:phr-repo";
	public const string KjernejournalResource = "nhn:kjernejournal";

	// This is the scope of the API you want to call (get an access token for)		

	//private const string ApiScopes = "nhn:phr/mhd/read-documentreferences nhn:phr/mhd/read-document nhn:phr/status nhn:phr/mhd/create-documents-with-reference nhn:phr/delete-documents-and-reference";
	//private const string ApiScopes = "nhn:phr/mhd/read-documentreferences nhn:phr/mhd/read-document nhn:phr/status nhn:phr/mhd/create-documents-with-reference nhn:phr/delete-documents-and-reference nhn:kjernejournal/innlogging nhn:kjernejournal/tillitsrammeverk";
	//private const string ApiScopes = "nhn:phr/mhd/read-documentreferences nhn:phr/mhd/read-document nhn:phr/status nhn:phr-document-repository/mhd/create-documents-with-reference nhn:phr-document-repository/delete-documents-and-reference nhn:phr-service/atna/read/document-repository nhn:phr-service/atna/read/document-repository-test nhn:kjernejournal/innlogging nhn:kjernejournal/tillitsrammeverk";	
	private const string ApiScopes = "nhn:phr/mhd/read-documentreferences nhn:phr/mhd/read-document nhn:phr/status nhn:phr-repo/mhd/create nhn:phr-repo/delete-docs-and-refs nhn:phr-repo/fhir/documentreference.read nhn:phr-svc/atna/r/repo nhn:phr-svc/atna/r/repo-test nhn:kjernejournal/innlogging nhn:kjernejournal/tillitsrammeverk";
	
	//private const string ApiScopes = "nhn:kjernejournal/innlogging nhn:kjernejournal/tillitsrammeverk";
	//private const string ApiScopesKjernejournalforskriften = "nhn:phr/mhd/read-documentreferences/kjernejournalforskriften nhn:phr/mhd/read-document nhn:phr/status nhn:phr/mhd/create-documents-with-reference nhn:phr/delete-documents-and-reference nhn:kjernejournal/innlogging nhn:kjernejournal/tillitsrammeverk";
	//private const string ApiScopesKjernejournalforskriften = "nhn:phr/mhd/read-documentreferences/kjernejournalforskriften nhn:phr/mhd/read-document nhn:phr/status nhn:phr-document-repository/mhd/create-documents-with-reference nhn:phr-document-repository/delete-documents-and-reference nhn:phr-service/atna/read/document-repository nhn:phr-service/atna/read/document-repository-test nhn:kjernejournal/innlogging nhn:kjernejournal/tillitsrammeverk";
	private const string ApiScopesKjernejournalforskriften = "nhn:phr/mhd/read-documentreferences/kjernejournalforskriften nhn:phr/mhd/read-document nhn:phr/status nhn:phr-repo/mhd/create nhn:phr-repo/delete-docs-and-refs nhn:phr-repo/fhir/documentreference.read nhn:phr-svc/atna/r/repo nhn:phr-svc/atna/r/repo-test nhn:kjernejournal/innlogging nhn:kjernejournal/tillitsrammeverk";

	//private const string ApiScopes = "nhn:phr/mhd/read-documentreferences nhn:phr/mhd/read-document nhn:phr/mhd/create-documents-with-reference nhn:hgd-persontjenesten-api/full-access";
	//private const string ApiScopesKjernejournalforskriften = "nhn:phr/mhd/read-documentreferences/kjernejournalforskriften nhn:phr/mhd/read-document nhn:phr/mhd/create-documents-with-reference nhn:hgd-persontjenesten-api/full-access";

	// These scopes indicate that you want an ID-token ("openid"), and what information about the user you want the ID-token to contain
	private const string IdentityScopes = "openid profile offline_access helseid://scopes/identity/pid helseid://scopes/identity/security_level";

	public const string TestRedirectUrl = "https://epj.pjd.test.nhn.no/callback";
	public const string TestPostLogoutRedirectUrl = "https://epj.pjd.test.nhn.no/";

	private const string ParentOrg = "958935420";   // Oslo kommune
													//private const string ParentOrg = "994598759"; 
	private const string ChildOrg = "883953142";    // BJØLSENHJEMMET
	private const string PatientPointOfCareOrg = "883953142";    // BJØLSENHJEMMET

	HttpClient httpClient;
	OidcClient oidcClient;
	DiscoveryDocumentResponse disco;

	public HomeController(
		IOptions<SessionOptions> sessionOptions,
		ILogger<HomeController> logger, 
		IHttpContextAccessor httpContextAccessor, 
		IMemoryCache memoryCache, 
		IOidcClientService oidcClientService,
		PhrApiOptions phrApiOptions,
		IPhrApiEnvironmentProvider phrApiEnvironmentProvider,
		OrganizationService organizationService,
		IUserTokenService userTokenService
		) : base(sessionOptions, userTokenService)
	{
		_logger = logger;
		_httpContextAccessor = httpContextAccessor;
		_memoryCache = memoryCache;
		_oidcClientService = oidcClientService;
		_organizationService = organizationService; 
		_phrApiOptions = phrApiOptions;
		_phrApiEnvironmentProvider = phrApiEnvironmentProvider;
	}
	
	public IActionResult Index(string patientIdentifier, string pid, string grunnlag) // Both "?patientIdentifier=..." and "?pid=..." can be used to set patient context
	{
		HomeViewModel model = new HomeViewModel();

		model.EnvironmentId = "test";

        var sessionData = session.SessionData(); 

		if (!string.IsNullOrEmpty(pid))
		{
			patientIdentifier = pid; 
		}

		if (!string.IsNullOrEmpty(patientIdentifier))
		{			
			return RedirectToAction(nameof(PatientController.ChoosePasient), "Patient", new { patientId = patientIdentifier, grunnlag = grunnlag });
		}

		if (sessionData.IsAuthenticated && sessionData.SourcePatientInfo?.NIN is null)
		{
			return RedirectToAction(nameof(PatientController.Pasientvalg), "Patient");		
		}

		model.AuthorizationId = sessionData.AuthorizationId;

		if (string.IsNullOrEmpty(model.AuthorizationId))
		{
			model.AuthorizationId = "LE"; // Default authorization - Lege
		}

		return View(model);
	}

	private string GetRedirectUrl()
	{
#if DEBUG
		var redirectUrl = Request.Scheme + "://" + Request.Host.Value + "/callback";
#else
		var redirectUrl = TestRedirectUrl; 
#endif
		return redirectUrl; 
	}

	private string GetPostLogoutRedirectUrl()
	{
#if DEBUG			
		var redirectUrl = Request.Scheme + "://" + Request.Host.Value + "/";
#else
		var redirectUrl = TestPostLogoutRedirectUrl; 
#endif
		return redirectUrl;
	}

	private async Task SetupOidcClient()
	{
		httpClient = new HttpClient();

		var redirectUrl = GetRedirectUrl(); 

		// Download the HelseID metadata from https://helseid-sts.test.nhn.no/.well-known/openid-configuration to determine endpoints and public keys used by HelseID:
		// In a production environment, this document must be cached for better efficiency (both for this client and for HelseID)
		disco = await httpClient.GetDiscoveryDocumentAsync(StsUrl);

		var options = new OidcClientOptions
		{
			Authority = StsUrl,
			ClientId = ClientId,
			RedirectUri = redirectUrl,
			LoadProfile = false,
			// This validates the identity token (important!):
			//IdentityTokenValidator = new JwtHandlerIdentityTokenValidator(),
		};

		// Set the DPoP proof, we can use the same key for this as for the client assertion:
		options.ConfigureDPoP(Constants.JwkPrivateKey);

		// Setup the oidc client for authentication against HelseID
		oidcClient = new OidcClient(options);
	}

	[HttpPost]
	public async Task<IActionResult> LoginStep1(HomeViewModel model)
	{
		try
		{				
			await SetupOidcClient();
			
			model.OrganizationName = HomeViewModel.OrganizationList.FirstOrDefault(x => x.Id == model.OrganizationId)!.Label;
			model.AuthorizationName = HomeViewModel.AuthorizationList.FirstOrDefault(x => x.Id == model.AuthorizationId)!.Label;
			
			model.EnvironmentName = PasientvalgViewModel.EnvironmentList.FirstOrDefault(x => x.Id == model.EnvironmentId)?.Label;

			var sessionData = session.SessionData();

			sessionData.AuthorizationId = model.AuthorizationId;
			sessionData.AuthorizationName = model.AuthorizationName;

			sessionData.EnvironmentId = model.EnvironmentId;
			sessionData.EnvironmentName = model.EnvironmentName;

			sessionData.DocumentListScopeId = model.DocumentListScopeId;

			switch (model.DocumentListScopeId)
			{
				case (int)DocumentListScopeEnum.HelsepersonellovenParagraf45:
					sessionData.DocumentListScopeName = "Helsepersonelloven \u00A7 45";
					break;

				case (int)DocumentListScopeEnum.Kjernejournalforskriften:
					sessionData.DocumentListScopeName = "Kjernejournalforskriften";
					break;
			}

			sessionData.SaveToSession(session);

			var attest = AttestModel.Build(model.OrganizationId, model.HealthcarePersonellUserId, model.AuthorizationId);

			model.AttestJson = BuildAttestJson(attest); 				 

			return View("Index", model);
		}
		catch (Exception ex)
		{
			await Console.Error.WriteLineAsync("Error:");
			await Console.Error.WriteLineAsync(ex.ToString());
		}

		return View();
	}
	

	[HttpPost]
	public async Task<IActionResult> LoginWithCustomizedAttest(HomeViewModel model)
	{
		try
		{
			var redirectUrl = GetRedirectUrl();
			
			Console.WriteLine("RedirectUrl: " + redirectUrl);

			await SetupOidcClient();

			// The authorizeState object contains the state that needs to be held between starting the authorize request and the response
			var authorizeState = await oidcClient.PrepareLoginAsync();

			var cacheEntryOptions = new MemoryCacheEntryOptions()
				.SetSlidingExpiration(TimeSpan.FromSeconds(120));
			
			var sessionData = session.SessionData();

			sessionData.AuthorizeState = JsonSerializer.Serialize(authorizeState);
			sessionData.SaveToSession(session);

			var scopes = sessionData.DocumentListScopeId == (int)DocumentListScopeEnum.HelsepersonellovenParagraf45 ? ApiScopes : ApiScopesKjernejournalforskriften; 

			var pushedAuthorizationResponse = await GetPushedAuthorizationResponse(model.AttestJson,
				httpClient,
				authorizeState,				
				disco,
				scopes,
				redirectUrl
				);

			if (pushedAuthorizationResponse.IsError)
			{
				throw new Exception($"{pushedAuthorizationResponse.Error}: JSON: {pushedAuthorizationResponse.Json}");
			}

			var startUrl = $"{disco.AuthorizeEndpoint}?client_id={ClientId}&request_uri={pushedAuthorizationResponse.RequestUri}";

			return Redirect(startUrl);				
			
		}
		catch (Exception ex)
		{
			await Console.Error.WriteLineAsync("Error:");
			await Console.Error.WriteLineAsync(ex.ToString());
		}

		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Login(HomeViewModel model)
	{
		try
		{
			var redirectUrl = GetRedirectUrl();

			await SetupOidcClient();

			model.OrganizationName = HomeViewModel.OrganizationList.FirstOrDefault(x => x.Id == model.OrganizationId)!.Label;
			model.AuthorizationName = HomeViewModel.AuthorizationList.FirstOrDefault(x => x.Id == model.AuthorizationId)!.Label;

			model.EnvironmentName = PasientvalgViewModel.EnvironmentList.FirstOrDefault(x => x.Id == model.EnvironmentId)?.Label;

			var sessionData = session.SessionData();

			sessionData.AuthorizationId = model.AuthorizationId;
			sessionData.AuthorizationName = model.AuthorizationName;

			sessionData.EnvironmentId = model.EnvironmentId;
			sessionData.EnvironmentName = model.EnvironmentName;

			sessionData.DocumentListScopeId = model.DocumentListScopeId;

			switch (model.DocumentListScopeId)
			{
				case (int)DocumentListScopeEnum.HelsepersonellovenParagraf45:
					sessionData.DocumentListScopeName = "Helsepersonelloven \u00A7 45";
					break;

				case (int)DocumentListScopeEnum.Kjernejournalforskriften:
					sessionData.DocumentListScopeName = "Kjernejournalforskriften";
					break;					
			}

			sessionData.SaveToSession(session);

			var attest = AttestModel.Build(model.OrganizationId, model.HealthcarePersonellUserId, model.AuthorizationId);

			var attestJson = BuildAttestJson(attest);

			// The authorizeState object contains the state that needs to be held between starting the authorize request and the response
			var authorizeState = await oidcClient.PrepareLoginAsync();

			var cacheEntryOptions = new MemoryCacheEntryOptions()
				.SetSlidingExpiration(TimeSpan.FromSeconds(120));

			sessionData.AuthorizeState = JsonSerializer.Serialize(authorizeState);
			sessionData.SaveToSession(session);

			var scopes = sessionData.DocumentListScopeId == (int)DocumentListScopeEnum.HelsepersonellovenParagraf45 ? ApiScopes : ApiScopesKjernejournalforskriften;

			var pushedAuthorizationResponse = await GetPushedAuthorizationResponse(attestJson,
				httpClient,
				authorizeState,
				disco,
				scopes,
				redirectUrl
				);

			if (pushedAuthorizationResponse.IsError)
			{
				throw new Exception($"{pushedAuthorizationResponse.Error}: JSON: {pushedAuthorizationResponse.Json}");
			}

			var startUrl = $"{disco.AuthorizeEndpoint}?client_id={ClientId}&request_uri={pushedAuthorizationResponse.RequestUri}";

			return Redirect(startUrl);

		}
		catch (Exception ex)
		{
			await Console.Error.WriteLineAsync("Error:");
			await Console.Error.WriteLineAsync(ex.ToString());
		}

		return View();
	}

	public IActionResult Logout()
	{
		// https://utviklerportal.nhn.no/informasjonstjenester/helseid/bruksmoenstre-og-eksempelkode/bruk-av-helseid/docs/teknisk-referanse/endepunkt/end-session-endepunktet_no_nbmd/

		var sessionData = session.SessionData();

		var helseIdLogoutUrl = string.Empty; // $"id_token_hint="

		switch (sessionData.EnvironmentId)
		{
			case "test":
			case "qa":
				helseIdLogoutUrl = $"https://helseid-sts.test.nhn.no/connect/endsession"; 
				break;

			case "prod":
				helseIdLogoutUrl = $"https://helseid-sts.nhn.no/connect/endsession";
				break; 
		}

		var postLogoutRedirectUrl = GetPostLogoutRedirectUrl();

		helseIdLogoutUrl += $"?id_token_hint={sessionData.IdentityToken}&post_logout_redirect_uri={postLogoutRedirectUrl}"; 
		
		session.Remove(Constants.SessionDataKey);

		//return RedirectToAction(nameof(Index));
		return Redirect(helseIdLogoutUrl); 
	}

	[Route("callback")]
	public async Task<IActionResult> Callback()
	{
		try
		{
			await SetupOidcClient();

			oidcClient.Options.ClientAssertion = GetClientAssertionPayload(disco);

			var sessionData = session.SessionData();

			var authorizeState = JsonSerializer.Deserialize<AuthorizeState>(sessionData.AuthorizeState); 

			//var authorizeState = _memoryCache.Get("authorizeState") as AuthorizeState;
			var helseIdResponse = Request.QueryString.ToString();

			// User login has finished, now we want to request tokens from the /token endpoint
			// We add a Resource parameter that indicates that we want scopes for PHR API. Later, we'll also get token with scopes for Kjernejournal. 

			var parameters = new Parameters
			{
				{"resource", PhrResource}
			};

			var loginResult = await oidcClient.ProcessResponseAsync(helseIdResponse, authorizeState, parameters);

			if (loginResult.IsError == false)
			{
				loginResult = ValidateIdentityClaims(loginResult);
			}

			if (loginResult.IsError)
			{
				throw new Exception($"{loginResult.Error}: Description: {loginResult.ErrorDescription}");
			}

			var handler = new JwtSecurityTokenHandler();
			var jsonIdentityToken = handler.ReadToken(loginResult.IdentityToken) as JwtSecurityToken;
			var jsonAccessToken = handler.ReadToken(loginResult.AccessToken) as JwtSecurityToken;

			if (jsonIdentityToken != null)
			{
				foreach (var claim in jsonIdentityToken.Claims)
				{
					Console.WriteLine($"{claim.Type}: {claim.Value}");
				}

				// Access specific claims
				var nameClaim = jsonIdentityToken.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Name)?.Value;
				Console.WriteLine($"Name: {nameClaim}");
			}

			// The access token can now be used when calling an api
			// It contains the user id, the security level, the organization number
			// and the child organization
			// Copy the access token and paste it at https://jwt.ms to decode it

			Console.WriteLine("First request, resource: " + PhrResource);
			Console.WriteLine($"Identity token from login: {loginResult.IdentityToken}");
			Console.WriteLine($"DPoP token from login: {loginResult.AccessToken}");

			sessionData.PjdAccessToken = loginResult.AccessToken;
			sessionData.PjdAccessTokenExpiresAt = new DateTimeOffset(jsonAccessToken.ValidTo);
			sessionData.RefreshToken = loginResult.RefreshToken;

			sessionData.SaveToSession(session); 

			//sessionData.AccessToken = loginResult.AccessToken;

			var pid = jsonIdentityToken.Claims.FirstOrDefault(c => c.Type == "helseid://claims/identity/pid")?.Value;

			//var sessionLoginResult = session.SessionLoginResult(); // Vi har allerede opprettet en instans i LoginStep1 for å lagre autorisasjon

			var environmentId = !string.IsNullOrEmpty(_phrApiOptions.ActiveEnvironment) ? _phrApiOptions.ActiveEnvironment : sessionData.EnvironmentId!; 

			_phrApiEnvironmentProvider.SetCurrentEnvironment(environmentId);

			sessionData.IsAuthenticated = true;
			sessionData.IdentityToken = loginResult.IdentityToken;
			//sessionData.AccessToken = loginResult.AccessToken;
			//sessionData.RefreshToken = loginResult.RefreshToken;
			sessionData.Name = loginResult.User.Identity!.Name!;
			sessionData.PersonId = pid;
			sessionData.HprNumber = jsonAccessToken.Claims.FirstOrDefault(c => c.Type == "helseid://claims/hpr/hpr_number")?.Value;

			var orgName = string.Empty;
			var orgIdentifier = string.Empty;
			var orgAssignOid = string.Empty;			
			
			//var givenName = loginResult.User.Claims.Where(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname").First().Value;
			var givenName = loginResult.User.Claims.Where(x => x.Type == "given_name").First().Value;
			var middleName = loginResult.User.Claims.Where(x => x.Type == "middle_name").First().Value;
			var familyName = loginResult.User.Claims.Where(x => x.Type == "family_name").First().Value;
			//var familyName = loginResult.User.Claims.Where(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname").First().Value;
			var authorName = $"{givenName}{" " + middleName} {familyName}";

			sessionData.GivenName = givenName;
			sessionData.MiddleName = middleName;
			sessionData.FamilyName = familyName;

			// We can now call the /token endpoint again with the refresh token in order to get a new access token:
			// Client assertions cannot be used twice, so we need a new payload:
			/*oidcClient.Options.ClientAssertion = GetClientAssertionPayload(disco);

			// We add a Resource parameter that indicates that we want scopes for Kjernejournal API
			parameters = new Parameters
			{
				{"resource", KjernejournalResource}
			};

			var refreshTokenResult = await oidcClient.RefreshTokenAsync(loginResult.RefreshToken, parameters);

			if (refreshTokenResult.IsError)
			{
				throw new Exception($"{refreshTokenResult.Error}: Description: {refreshTokenResult.ErrorDescription}");
			}				

			Console.WriteLine();
			Console.WriteLine("Second request, resource: " + KjernejournalResource);
			Console.WriteLine("New Access Token after using Refresh Token:");
			Console.WriteLine(refreshTokenResult.AccessToken);
			Console.WriteLine(refreshTokenResult.RefreshToken);

			sessionData.KjernejournalAccessToken = refreshTokenResult.AccessToken; */

			// Retrieve submission's AUTHOR-ORG details
			//var org = new Hl7.Fhir.Model.Organization();

			try
			{
				var claimClientOrgNr = jsonAccessToken.Claims.Where(x => x.Type == "helseid://claims/client/claims/orgnr_parent").First()!.Value;
				//var claimClientOrgNr = jsonAccessToken.Claims.Where(x => x.Type == "helseid://claims/client/claims/orgnr_child").First()!.Value;
				//var claimClientOrgNr = jsonAccessToken.Claims.Where(x => x.Type == "helseid://claims/client/claims/orgnr_supplier").First()!.Value;

				var organization = await _organizationService.GetOrganizationByOrgNrAsync(claimClientOrgNr);

				sessionData.OrganizationName = organization?.name;
				sessionData.OrganizationId = organization?.id;

				sessionData.OrgRegOrganization = organization; 

				/*org = await _organizationService.GetHl7OrganizationDetailsAsync(claimClientOrgNr);
				
				orgName = organization.name; //.Name;

				foreach (var ident in org.Identifier)
				{
					orgIdentifier = ident.Value;
					orgAssignOid = ident.System;
				}*/
			}
			catch
			{
				_logger.LogError($"{nameof(HomeController)}.{nameof(Callback)}: Could not get organization details"); 
			}			

			sessionData.SaveToSession(session); 							
		}
		catch (Exception ex)
		{
			await Console.Error.WriteLineAsync("Error:");
			await Console.Error.WriteLineAsync(ex.ToString());
		}

		return RedirectToAction(nameof(Index));
	}
	
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}


	private static async Task<PushedAuthorizationResponse> GetPushedAuthorizationResponse(
		//AttestModel attest,
		string attestJson,
		HttpClient httpClient,
		AuthorizeState authorizeState,
		DiscoveryDocumentResponse disco,
		string scopes, 
		string redirectUrl
		)
	{
		// Sets the pushed authorization request parameters:
		/*var challengeBytes = SHA256.HashData(Encoding.UTF8.GetBytes(authorizeState.CodeVerifier));
		var codeChallenge = WebEncoders.Base64UrlEncode(challengeBytes);
		var requestObject = BuildRequestObject(disco, attestJson);		*/

		/*var dictionary = new Dictionary<string, string>
		{			
			{ "redirect_uri", redirectUrl}			
		};
		var parameters = new Parameters(dictionary);*/

		// Setup a client assertion - this will authenticate the client (this application)
		var clientAssertionPayload = GetClientAssertionPayload(disco);
		var pushedAuthorizationRequest = new PushedAuthorizationRequest
		{
			//Resource = new List<string> { PhrResource, KjernejournalResource },
			Address = disco.PushedAuthorizationRequestEndpoint,
			/*ClientId = ClientId,
			ClientAssertion = clientAssertionPayload,
			Request = requestObject,
			Scope = scopes,
			RedirectUri = redirectUrl,			
			ResponseType = OidcConstants.ResponseTypes.Code,
			ClientCredentialStyle = ClientCredentialStyle.PostBody,
			CodeChallenge = codeChallenge,
			CodeChallengeMethod = OidcConstants.CodeChallengeMethods.Sha256,
			State = authorizeState.State,
			Parameters = parameters*/
			Parameters = GetParametersForPushedAuthorizationRequest(
				//attest,
				attestJson,
				clientAssertionPayload,
				authorizeState,
				disco,
				scopes,
				redirectUrl
				),
		};

		// Calls the /par endpoint in order to get a request URI for the /authorize endpoint
		return await httpClient.PushAuthorizationAsync(pushedAuthorizationRequest);
	}

	// This is necessary in order to please the IdentityModel library:
	private static Parameters GetParametersForPushedAuthorizationRequest(
		//AttestModel attest,
		string attestJson,
		ClientAssertion clientAssertionPayload,
		AuthorizeState authorizeState,
		DiscoveryDocumentResponse disco,
		string scopes, 
		string redirectUrl
		)
	{
		// Sets the pushed authorization request parameters:
		var challengeBytes = SHA256.HashData(Encoding.UTF8.GetBytes(authorizeState.CodeVerifier));
		var codeChallenge = WebEncoders.Base64UrlEncode(challengeBytes);
		var requestObject = BuildRequestObject(disco, attestJson);

		var parameters = new Parameters(); 
		parameters.Add("resource", PhrResource);
		parameters.Add("resource", PhrServiceResource, parameterReplace: ParameterReplaceBehavior.None);
		parameters.Add("resource", PhrRepositoryResource, parameterReplace: ParameterReplaceBehavior.None);
		parameters.Add("resource", KjernejournalResource, parameterReplace: ParameterReplaceBehavior.None);
		parameters.Add("client_id", ClientId);
		parameters.Add("client_assertion", clientAssertionPayload.Value);
		parameters.Add("client_assertion_type", clientAssertionPayload.Type);
		parameters.Add("request", requestObject);
		parameters.Add("scope", scopes + " " + IdentityScopes);
		parameters.Add("redirect_uri", redirectUrl);
		parameters.Add("response_type", OidcConstants.ResponseTypes.Code);
		parameters.Add("code_challenge", codeChallenge);
		parameters.Add("code_challenge_method", OidcConstants.CodeChallengeMethods.Sha256);
		parameters.Add("state", authorizeState.State);

		return parameters; 
		
		/*var dictionary = new Dictionary<string, string>
		{
			{ "resource", PhrResource },
			{ "resource", KjernejournalResource },
			{ "client_id", ClientId },
			{ "client_assertion", clientAssertionPayload.Value },
			{ "client_assertion_type", clientAssertionPayload.Type },
			{ "request", requestObject },
			{ "scope", scopes + " " + IdentityScopes },
			{ "redirect_uri", redirectUrl},
			{ "response_type", OidcConstants.ResponseTypes.Code },
			{ "code_challenge", codeChallenge },
			{ "code_challenge_method", OidcConstants.CodeChallengeMethods.Sha256 },
			{ "state", authorizeState.State },
		};
		return new Parameters(dictionary);*/
	}

	private static string BuildAttestJson(AttestModel attest, bool writeIndented = true)
	{
		/* helseid_authorization */
		var orgNumberDetails = new Dictionary<string, string>
		{
			{"system", "urn:oid:1.0.6523"},
			{"type", "ENH"},
			{
                // The client configuration in HelseID contains a delegation from the supplier of this system to the
                // requested organization number. If this delegation is not set, the request will be invalidated.
				//"value", $"NO:ORGNR:{attest.LegalIdentity}:{ChildOrg}"
				"value", $"NO:ORGNR:{attest.LegalIdentity}:{attest.PointOfCare}"
			}
		};

		var orgNumberIdentifier = new Dictionary<string, object>
		{
			{"identifier", orgNumberDetails}
		};

		var organization = new Dictionary<string, object>
		{
			{"organization", orgNumberIdentifier}
		};

		var helseIdAuthorizationDetails = new Dictionary<string, object>
		{
			{"type", "helseid_authorization"},
			{"practitioner_role", organization}
		};

		/* practioner */

		/*var identifier = new Dictionary<string, object>
	{
		{"id", attest.Identifier},
		{"system", "urn:oid:2.16.578.1.12.4.1.4.1"},
	};*/

		var practioner_authorization = new Dictionary<string, object>
	{
		{"code", attest.Authorization},
		{"system", "urn:oid:2.16.578.1.12.4.1.1.9060"},
	};

		var legal_entity = new Dictionary<string, object>
	{
		{"id", attest.LegalIdentity},
		{"system", "urn:oid:2.16.578.1.12.4.1.4.101"},
	};

		var point_of_care = new Dictionary<string, object>
	{
		{"id", attest.PointOfCare},
		{"system", "urn:oid:2.16.578.1.12.4.1.4.101"},
	};

		var department = new Dictionary<string, object>
	{
		{"id", attest.Department},    // Avdeling ved Sykehus
		{"system", "urn:oid:2.16.578.1.12.4.1.4.102"},
	};

		var practioner = new Dictionary<string, object>
	{
		//{"identifier", identifier},
            {"authorization", practioner_authorization},
		{"legal_entity", legal_entity},
		{"point_of_care", point_of_care},
		//{"department", department},
	};

		/* care_relationship */

		var healthcare_service = new Dictionary<string, object>
	{
		{"code", attest.HealthcareService}, // 
		{"system", "urn:oid:2.16.578.1.12.4.1.1.8666"},
	};

		var purpose_of_use = new Dictionary<string, object>
	{
		{"code", attest.PurposeOfUse},
		{"system", "urn:oid:2.16.840.1.113883.1.11.20448"},
		//{"code", "1"},
		//{"system", "urn:oid:1.0.14265.1"},			
	};

		var purpose_of_use_details = new Dictionary<string, object>
	{
		{"code", attest.PurposeOfUseDetails},
		{"system", "urn:AuditEventHL7Norway/CodeSystem/carerelation"},
	};

		var decision_ref = new Dictionary<string, object>
	{
		{"id", Guid.NewGuid().ToString()},
		{"user_selected", true},
	};

		var care_relationship = new Dictionary<string, object>
	{
		{"healthcare_service", healthcare_service},
		{"purpose_of_use", purpose_of_use},
		{"purpose_of_use_details", purpose_of_use_details},
		{"decision_ref", decision_ref},
	};

		/* patients */

		var patient_point_of_care = new Dictionary<string, object>
	{
		{"id", attest.PatientPointOfCare},
		{"system", "urn:oid:2.16.578.1.12.4.1.4.101"},
	};

		var patient_department = new Dictionary<string, object>
	{
		{"id", attest.PatientDepartment},
		{"system", "urn:oid:2.16.578.1.12.4.1.4.102"},
	};

		var patient = new Dictionary<string, object>
	{
		{"point_of_care", patient_point_of_care},
            //{"department", patient_department},
        };

		var patients = new List<object>() { patient };

		var tillitsrammeverkDetails = new Dictionary<string, object>
		{
			{"type", "nhn:tillitsrammeverk:parameters"},
			{"practitioner", practioner},
			//{"practitioner_role", organization},
			{"care_relationship", care_relationship},
			{"patients", patients},
		};
		
		/*var acp = new Dictionary<string, object>
		{
			{"xua-acp", "2.16.578.1.12.4.1.7.2.1.6"},
		};

		var acpObj = new Dictionary<string, object>
		{
			{"type", "xua-acp"},
			{"value", acp},
		};*/

		var authorizationDetailsList = new List<Dictionary<string, object>>
		{
			helseIdAuthorizationDetails,
			tillitsrammeverkDetails
		};

		var serialized = JsonSerializer.Serialize(authorizationDetailsList, new JsonSerializerOptions() { WriteIndented = writeIndented });

		return serialized;
	}

	private static string BuildRequestObject(DiscoveryDocumentResponse disco, string attestJson /* AttestModel attest*/)
	{
		var claims = new List<Claim>
	{
		new Claim("authorization_details", attestJson, "json"),
		new Claim(JwtClaimTypes.Subject, ClientId),
		new Claim(JwtClaimTypes.IssuedAt, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
		new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N")),
	};

		return BuildHelseIdJwt(disco, claims);
	}

	public static ClientAssertion GetClientAssertionPayload(DiscoveryDocumentResponse disco)
	{
		var clientAssertion = BuildClientAssertion(disco);

		return new ClientAssertion
		{
			Type = OidcConstants.ClientAssertionTypes.JwtBearer,
			Value = clientAssertion,
		};
	}

	private static string BuildClientAssertion(DiscoveryDocumentResponse disco)
	{
		var claims = new List<Claim>
	{
		new Claim(JwtClaimTypes.Subject, ClientId),
		new Claim(JwtClaimTypes.IssuedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
		new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N")),
	};

		return BuildHelseIdJwt(disco, claims);
	}

	private static string BuildHelseIdJwt(DiscoveryDocumentResponse disco, List<Claim> extraClaims)
	{
		var signingCredentials = GetClientAssertionSigningCredentials();

		// Create a custom header with "typ" set to "client-authentication+jwt"
		// Required according to updated requirements from HelseId
		// https://utviklerportal.nhn.no/informasjonstjenester/helseid/bruksmoenstre-og-eksempelkode/bruk-av-helseid/docs/tekniske-mekanismer/bruk_av_client_assertion_enmd/
		var header = new JwtHeader(signingCredentials); 

		header["typ"] = "client-authentication+jwt"; // Override the default "JWT"

		var credentials = new JwtSecurityToken(
			header,
			new JwtPayload(ClientId, disco.Issuer, extraClaims, DateTime.UtcNow, DateTime.UtcNow.Add(HelseIdJwtLifetime))
		);

		var tokenHandler = new JwtSecurityTokenHandler();
		return tokenHandler.WriteToken(credentials);
	}

	private static SigningCredentials GetClientAssertionSigningCredentials()
	{
		var securityKey = new JsonWebKey(Constants.JwkPrivateKey);
		return new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
	}

	private static LoginResult ValidateIdentityClaims(LoginResult loginResult)
	{
		// The claims from the identity token has ben set on the User object;
		// We validate that the user claims match the required security level:
		if (loginResult.User.Claims.Any(c => c is
			{
				Type: "helseid://claims/identity/security_level",
				Value: "4",
			}))
		{
			return loginResult;
		}

		return new LoginResult("Invalid security level", "The security level is not at the required value");
	}
}
