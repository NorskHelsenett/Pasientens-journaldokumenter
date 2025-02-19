using IdentityModel;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.DPoP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using PatientHealthRecordsEPJ.Models;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PatientHealthRecordsEPJ.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private ISession session => _httpContextAccessor.HttpContext!.Session;
		private readonly IMemoryCache _memoryCache;
		
		private const string ClientId = "2141d734-7604-42ca-a377-27f1b712f4e6"; // "Patient Health Records API EPJ". Privatnøkkel ligger i Constants.cs

		// The client is configured in the HelseID test environment, so we will point to that
		private const string StsUrl = "https://helseid-sts.test.nhn.no";
				
		// This is the scope of the API you want to call (get an access token for)		
		private const string ApiScopes = "nhn:phr/mhd/read-documentreferences nhn:phr/mhd/read-document";
		private const string ApiScopesKjernejournalforskriften = "nhn:phr/mhd/read-documentreferences/kjernejournalforskriften nhn:phr/mhd/read-document";

		// These scopes indicate that you want an ID-token ("openid"), and what information about the user you want the ID-token to contain
		private const string IdentityScopes = "openid profile offline_access helseid://scopes/identity/pid helseid://scopes/identity/security_level";

		private const string TestRedirectUrl = "https://epj.pjd.test.nhn.no/callback";
		private const string TestPostLogoutRedirectUrl = "https://epj.pjd.test.nhn.no/";

		HttpClient httpClient;
		OidcClient oidcClient;
		DiscoveryDocumentResponse disco;

		public HomeController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
		{
			_logger = logger;
			_httpContextAccessor = httpContextAccessor;
			_memoryCache = memoryCache;
		}

		public IActionResult Index()
		{
			HomeViewModel model = new HomeViewModel();

            model.EnvironmentId = "test";

            var sessionLoginResult = session.SessionLoginResult(); 

			if (sessionLoginResult.IsAuthenticated && sessionLoginResult.PatientId is null)
			{
				return RedirectToAction(nameof(Pasientvalg));
			}

			model.AuthorizationId = "LE"; // Default authorization - Lege

			return View(model);
		}

		public IActionResult Pasientvalg()
		{
			PasientvalgViewModel model = new PasientvalgViewModel();

			model.EnvironmentId = "test";
			model.GrunnlagId = "akutt";

			var sessionLoginResult = session.SessionLoginResult();

			model.PatientId = sessionLoginResult.PatientId ?? string.Empty; 		

			if (string.IsNullOrEmpty(model.PatientId))
			{
				model.PatientId = "12119000465"; // Malin Fos Bogen som standard
				//model.PatientId = "13116900216"; // Line Danser som standard
			}

			return View(model);
		}

		[HttpPost]
		public IActionResult VelgPasient(PasientvalgViewModel model)
		{				
			var sessionLoginResult = session.SessionLoginResult();

			sessionLoginResult.PatientId = model.PatientId;
			sessionLoginResult.PatientName = PasientvalgViewModel.PatientMappingList.FirstOrDefault(x => x.Id == model.PatientId)?.Label;
			sessionLoginResult.GrunnlagId = model.GrunnlagId;
			sessionLoginResult.GrunnlagName = PasientvalgViewModel.GrunnlagList.FirstOrDefault(x => x.Id == model.GrunnlagId)?.Label;
			//sessionLoginResult.EnvironmentId = model.EnvironmentId;
			//sessionLoginResult.EnvironmentName = PasientvalgViewModel.EnvironmentList .FirstOrDefault(x => x.Id == model.EnvironmentId)?.Label;
			
			sessionLoginResult.SaveToSession(session); 			

			return Redirect("/"); 
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
				IdentityTokenValidator = new JwtHandlerIdentityTokenValidator(),
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

				var sessionLoginResult = session.SessionLoginResult();

				sessionLoginResult.AuthorizationId = model.AuthorizationId;
				sessionLoginResult.AuthorizationName = model.AuthorizationName;

				sessionLoginResult.EnvironmentId = model.EnvironmentId;
				sessionLoginResult.EnvironmentName = model.EnvironmentName;

				sessionLoginResult.DocumentListScopeId = model.DocumentListScopeId;

				switch (model.DocumentListScopeId)
				{
					case (int)DocumentListScopeEnum.HelsepersonellovenParagraf45:
						sessionLoginResult.DocumentListScopeName = "Helsepersonelloven § 45";
						break;

					case (int)DocumentListScopeEnum.Kjernejournalforskriften:
						sessionLoginResult.DocumentListScopeName = "Kjernejournalforskriften";
						break;
				}

				sessionLoginResult.SaveToSession(session);

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
				
				var sessionLoginResult = session.SessionLoginResult();

				sessionLoginResult.AuthorizeState = JsonSerializer.Serialize(authorizeState);
				sessionLoginResult.SaveToSession(session);

				var scopes = sessionLoginResult.DocumentListScopeId == (int)DocumentListScopeEnum.HelsepersonellovenParagraf45 ? ApiScopes : ApiScopesKjernejournalforskriften; 

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

				var sessionLoginResult = session.SessionLoginResult();

				sessionLoginResult.AuthorizationId = model.AuthorizationId;
				sessionLoginResult.AuthorizationName = model.AuthorizationName;

				sessionLoginResult.EnvironmentId = model.EnvironmentId;
				sessionLoginResult.EnvironmentName = model.EnvironmentName;
				
				sessionLoginResult.DocumentListScopeId = model.DocumentListScopeId;

				switch (model.DocumentListScopeId)
				{
					case (int)DocumentListScopeEnum.HelsepersonellovenParagraf45:
						sessionLoginResult.DocumentListScopeName = "Helsepersonelloven § 45";
						break;

					case (int)DocumentListScopeEnum.Kjernejournalforskriften:
						sessionLoginResult.DocumentListScopeName = "Kjernejournalforskriften";
						break;					
				}

				sessionLoginResult.SaveToSession(session);

				var attest = AttestModel.Build(model.OrganizationId, model.HealthcarePersonellUserId, model.AuthorizationId);

				var attestJson = BuildAttestJson(attest);

				// The authorizeState object contains the state that needs to be held between starting the authorize request and the response
				var authorizeState = await oidcClient.PrepareLoginAsync();

				var cacheEntryOptions = new MemoryCacheEntryOptions()
					.SetSlidingExpiration(TimeSpan.FromSeconds(120));
				
				sessionLoginResult.AuthorizeState = JsonSerializer.Serialize(authorizeState);
				sessionLoginResult.SaveToSession(session);

				var scopes = sessionLoginResult.DocumentListScopeId == (int)DocumentListScopeEnum.HelsepersonellovenParagraf45 ? ApiScopes : ApiScopesKjernejournalforskriften;

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

			var sessionLoginResult = session.SessionLoginResult();

			var helseIdLogoutUrl = string.Empty; // $"id_token_hint="

			switch (sessionLoginResult.EnvironmentId)
			{
				case "test":
					helseIdLogoutUrl = $"https://helseid-sts.test.nhn.no/connect/endsession"; 
					break;

				case "prod":
					helseIdLogoutUrl = $"https://helseid-sts.nhn.no/connect/endsession";
					break; 
			}

			var postLogoutRedirectUrl = GetPostLogoutRedirectUrl();

			helseIdLogoutUrl += $"?id_token_hint={sessionLoginResult.IdentityToken}&post_logout_redirect_uri={postLogoutRedirectUrl}"; 
			
			session.Remove(Constants.SessionLoginResultKey);

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

				var sessionLoginResult = session.SessionLoginResult();

				var authorizeState = JsonSerializer.Deserialize<AuthorizeState>(sessionLoginResult.AuthorizeState); 

				//var authorizeState = _memoryCache.Get("authorizeState") as AuthorizeState;
				var helseIdResponse = Request.QueryString.ToString();

				var loginResult = await oidcClient.ProcessResponseAsync(helseIdResponse, authorizeState);

				if (loginResult.IsError == false)
				{
					loginResult = ValidateIdentityClaims(loginResult);
				}

				if (loginResult.IsError)
				{
					throw new Exception($"{loginResult.Error}: Description: {loginResult.ErrorDescription}");
				}

				var handler = new JwtSecurityTokenHandler();
				var jsonToken = handler.ReadToken(loginResult.IdentityToken) as JwtSecurityToken;

				if (jsonToken != null)
				{
					foreach (var claim in jsonToken.Claims)
					{
						Console.WriteLine($"{claim.Type}: {claim.Value}");
					}

					// Access specific claims
					var nameClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Name)?.Value;
					Console.WriteLine($"Name: {nameClaim}");
				}

				// The access token can now be used when calling an api
				// It contains the user id, the security level, the organization number
				// and the child organization
				// Copy the access token and paste it at https://jwt.ms to decode it

				Console.WriteLine($"Identity token from login: {loginResult.IdentityToken}");
				Console.WriteLine($"DPoP token from login: {loginResult.AccessToken}");

				var pid = jsonToken.Claims.FirstOrDefault(c => c.Type == "helseid://claims/identity/pid")?.Value;

				//var sessionLoginResult = session.SessionLoginResult(); // Vi har allerede opprettet en instans i LoginStep1 for å lagre autorisasjon

				sessionLoginResult.IsAuthenticated = true;
				sessionLoginResult.IdentityToken = loginResult.IdentityToken;
				sessionLoginResult.AccessToken = loginResult.AccessToken;
				sessionLoginResult.RefreshToken = loginResult.RefreshToken;
				sessionLoginResult.Name = loginResult.User.Identity!.Name!;
				sessionLoginResult.PersonId = pid; 

				// We can now call the /token endpoint again with the refresh token in order to get a new access token:
				// Client assertions cannot be used twice, so we need a new payload:
				oidcClient.Options.ClientAssertion = GetClientAssertionPayload(disco);

				var refreshTokenResult = await oidcClient.RefreshTokenAsync(loginResult.RefreshToken);

				if (refreshTokenResult.IsError)
				{
					throw new Exception($"{refreshTokenResult.Error}: Description: {refreshTokenResult.ErrorDescription}");
				}

				Console.WriteLine();
				Console.WriteLine("New Access Token after using Refresh Token:");
				Console.WriteLine(refreshTokenResult.AccessToken);

				sessionLoginResult.SaveToSession(session); 							
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
			// Setup a client assertion - this will authenticate the client (this application)
			var clientAssertionPayload = GetClientAssertionPayload(disco);
			var pushedAuthorizationRequest = new PushedAuthorizationRequest
			{
				Address = disco.PushedAuthorizationRequestEndpoint,
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
			
			var dictionary = new Dictionary<string, string>
		{
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
			return new Parameters(dictionary);
		}

		private static string BuildAttestJson(AttestModel attest, bool writeIndented = true)
		{
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

			var authorizationDetails = new Dictionary<string, object>
		{
			{"type", "nhn:tillitsrammeverk:parameters"},
			{"practitioner", practioner},
			//{"practitioner_role", organization},
			{"care_relationship", care_relationship},
			{"patients", patients},
		};

			var serialized = JsonSerializer.Serialize(authorizationDetails, new JsonSerializerOptions() { WriteIndented = writeIndented });

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

		private static ClientAssertion GetClientAssertionPayload(DiscoveryDocumentResponse disco)
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
			new Claim(JwtClaimTypes.IssuedAt, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
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
				new JwtPayload(ClientId, disco.Issuer, extraClaims, DateTime.UtcNow, DateTime.UtcNow.AddSeconds(60))
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
}
