using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using IdentityModel.OidcClient.DPoP;

namespace HelseId.Samples.SimpleRequestObjectsDemo;

class Program
{
	//
	// This sample is an adaption of the HelseID samples at https://github.com/NorskHelsenett/HelseID.Samples/tree/master/NativeClients
    //
	// The values below should normally be configurable
	//

	// In a production environment you MUST use your own private key stored somewhere safe (a vault, a certificate store, a secure database or similar).	    

	// This private key is only to be used for this particular sample. Your application will use it's own private key.

	private const string JwkPrivateKey = """ 
                {
            "p": "_8WHFd_pMbnVMwNVn1zxuK3SOrTb0yQyq6j3yQ2bTkOGnZ3Qqh760fDcyDrW4h_Hhy2ssbPbuwyNFf9j5boDQ1R3IZL8hFq-dkF55pHGRMSBt0OVQYWu79mfU8KVaeYi439N9Hgt_XRl5J8VOaxN2sx1tWe4Gbjy9z6IEyJ8LgM",
            "kty": "RSA",
            "q": "iRR5niXtUmxH2cUNQCq5WhzMEvpAC5BbgBEyG0KjTX7RflbJmNflb-lwwLSNsZROGSAHra3rYvA5oXfsbdlIbkOmOpV6AeySAwYPjTpp5E2qx4Jnc7v23Fl9hAfeQLJp3Gsg1lFs1COUXZmvzA06mI2Xor5gyHiTC1MOjmu4CZ0",
            "d": "EJ2l3QwmSiehnBHSdagMpk1_TomKLS9aMp91lJHXJpm6NAg_WWml0S41uE1rZE0veQBIeODeLtV9PR4J6S46XanqKiHGT4os_WLJIMD4yZGH05Hbot2jb9rfif5uRYVJ4oEBAaZdTmr0UFDqAn3PCwx2YziW6EOrAeQU5-XLFPlvFhscLJhTonZpDL6soQBMyJUV57a6-6Lsm3iNxbCd-x_DaZSyS4iB0RxBXGwyvWPL8846L6Ch87Tt2PAa1lVRb4YZgCmOiar9oPtRe6GI2lbyNGtVkxi6UXGpGMdeyXRRRhfuGGmdq0z1RWTl_fw32Y6vHNnrLhk66LPha-pgSQ",
            "e": "AQAB",
            "use": "sig",
            "kid": "oIAHPtvFo3z2jc1NV4x8FUOTGkDhyer2o6_9cxWV72o",
            "qi": "30xJ0V11tvtVI1--woxliXKx9aUpNZ3zQ0KgmiAZSvP2Y6bNccLwMnb5I8ctLocRiazeIrvNT7LwB4Kdc_hCyE0TxqC9pWKevMtzwVKReSRmO2Zp7OGty1gzSpd7ZmZNbJGLpCWvKkB34AiVJLvq9Xk9B7xB8oQ0oPMYjfNklA8",
            "dp": "XQ1dBw1ORqQIN6X36aGj43-Bo5AXBar-sEsp0sCbhw60E_XNa4cX03dgq4wUj3HmYnywfnoj79oyHhWrH3HcLAu3x8Q1EGX_MwDBO6w_SNeBJc51p7_eFC7Fc3CwcTWj7cZr8wwiFzrYsyNZUoEoVfjaviO868bIxkC-us9qBEc",
            "alg": "RS256",
            "dq": "OKiN3CAxB3BaAljAMVV3ZxegAfgNoLn6F-UDoODaPp28WUvl55bU7ny-begy6MMzxI7InbDVKf_A0kyPHJhCp9CuVGiUFVeCwl0u8GDEg-jWPcBYoi1-y3TxvDDEXFliCAj_eZYONWC_YjCzyNNu9L8RfeLSfgnYPUYHLja3ysE",
            "n": "iPUqO6EwqOdtCtUIpO40basdlyD18hNc412alGkVKvJ4ieIJZAEiYY7z0FIHVPytKZ-FLD_6Ut10aZVlXSanfIQAxjQ89Z2v0Jk1-toGU_sFq4EeEDQ7zlyk4KvQFsgN5wsoG0hxOwafvTK4mYKsaVVWLoYzvHgB-2YPANPTJE8RQZ6juLWmdt2S0yGW2bRW2kMSf8Rpnaj_ufGFudTtNbLTdVzbhaoMZJ_YnOOyX8b6u5E2hN3ucWxCpnsseEmHcWYP5CyvybrlI6KlQDo4tvweziZ_86S_nbeJJa56CW_DcEA9tdvLmi_q6g2K_Dhe7ndb7uFHrfqYq4AY1u5S1w"
        }
        """;

	// This client_id is only to be used for this particular sample. Your application will use it's own client_id.
	private const string ClientId = "2141d734-7604-42ca-a377-27f1b712f4e6"; // "Patient Health Records API EPJ"	

	// The client is configured in the HelseID test environment, so we will point to that
	private const string StsUrl = "https://helseid-sts.test.nhn.no";

	private const string PatientId = "13116900216";
	//private const string ApiUrl = "https://api.pjd.test.nhn.no/R4/fhir/documentreference/_search";
	private const string ApiUrl = "https://localhost:8443/R4/fhir/documentreference/_search";

	//private const int LocalhostPort = 8080;
	private const int LocalhostPort = 8089;

	// In a test environment, the port does not need to be pre-registered in HelseID Selvbetjening;
	// this means that you can allocate any available port when launching the application:
	//private static readonly string RedirectUrl = $"https://localhost:{LocalhostPort.ToString()}/callback";
	private static readonly string RedirectUrl = $"http://127.0.0.1:{LocalhostPort.ToString()}/callback";

	private static bool useKjernejournalforskriften = true;  // Set to true to call API with ApiScopesKjernejournalforskriften and hit-access-basis

	// This is the scope of the API you want to call (get an access token for)	
	private const string ApiScopes = "nhn:phr/mhd/read-documentreferences";
	private const string ApiScopesKjernejournalforskriften = "nhn:phr/mhd/read-documentreferences/kjernejournalforskriften";

	// These scopes indicate that you want an ID-token ("openid"), and what information about the user you want the ID-token to contain
	private const string IdentityScopes = "openid profile offline_access helseid://scopes/identity/pid helseid://scopes/identity/security_level";

    static async Task Main(string[] args)
    {
        try
        {
            using var httpClient = new HttpClient();

            // Download the HelseID metadata from https://helseid-sts.test.nhn.no/.well-known/openid-configuration to determine endpoints and public keys used by HelseID:
            // In a production environment, this document must be cached for better efficiency (both for this client and for HelseID)
            var disco = await httpClient.GetDiscoveryDocumentAsync(StsUrl);

            var options = new OidcClientOptions
            {
                Authority = StsUrl,
                ClientId = ClientId,
                RedirectUri = RedirectUrl,
                LoadProfile = false,
                // This validates the identity token (important!):
                IdentityTokenValidator = new JwtHandlerIdentityTokenValidator(),
            };

            // Set the DPoP proof, we can use the same key for this as for the client assertion:
            options.ConfigureDPoP(JwkPrivateKey);

            // Setup the oidc client for authentication against HelseID
            var oidcClient = new OidcClient(options);

            // The authorizeState object contains the state that needs to be held between starting the authorize request and the response
            var authorizeState = await oidcClient.PrepareLoginAsync();

            var pushedAuthorizationResponse = await GetPushedAuthorizationResponse(
                httpClient,
                authorizeState,
                disco);

            if (pushedAuthorizationResponse.IsError)
            {
                throw new Exception($"{pushedAuthorizationResponse.Error}: JSON: {pushedAuthorizationResponse.Json}");
            }

            var startUrl = $"{disco.AuthorizeEndpoint}?client_id={ClientId}&request_uri={pushedAuthorizationResponse.RequestUri}";

            var browserOptions = new BrowserOptions(startUrl, RedirectUrl);

            // Create a redirect URI using an available port on the loopback address.
            var browser = new SystemBrowser(port: LocalhostPort);

            var browserResult = await browser.InvokeAsync(browserOptions, default);

            // If the result type is success, the browser result should contain the authorization code.
            // We can now call the /token endpoint with the authorization code in order to get tokens:

            // We need a client assertion on the request in order to authenticate the client:
            oidcClient.Options.ClientAssertion = GetClientAssertionPayload(disco);

            var loginResult = await oidcClient.ProcessResponseAsync(browserResult.Response, authorizeState);

            if (loginResult.IsError == false)
            {
                loginResult = ValidateIdentityClaims(loginResult);
            }

            if (loginResult.IsError)
            {
                throw new Exception($"{loginResult.Error}: Description: {loginResult.ErrorDescription}");
            }

            // The access token can now be used when calling an api
            // It contains the user id, the security level, the organization number
            // and the child organization
            // Copy the access token and paste it at https://jwt.ms to decode it

            Console.WriteLine($"Identity token from login: {loginResult.IdentityToken}");            
			Console.WriteLine($"DPoP token from login: {loginResult.AccessToken}");

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


			// Call the Patient Health Records API 
			await CallApi(loginResult.AccessToken);
		}
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync("Error:");
            await Console.Error.WriteLineAsync(ex.ToString());
        }
    }

    private static async Task<PushedAuthorizationResponse> GetPushedAuthorizationResponse(
        HttpClient httpClient,
        AuthorizeState authorizeState,
        DiscoveryDocumentResponse disco)
    {
        // Setup a client assertion - this will authenticate the client (this application)
        var clientAssertionPayload = GetClientAssertionPayload(disco);
        var pushedAuthorizationRequest = new PushedAuthorizationRequest
        {
            Address = disco.PushedAuthorizationRequestEndpoint,
            Parameters = GetParametersForPushedAuthorizationRequest(
                clientAssertionPayload,
                authorizeState,
                disco),
        };

        // Calls the /par endpoint in order to get a request URI for the /authorize endpoint
        return await httpClient.PushAuthorizationAsync(pushedAuthorizationRequest);
    }

    // This is necessary in order to please the IdentityModel library:
    private static Parameters GetParametersForPushedAuthorizationRequest(
        ClientAssertion clientAssertionPayload,
        AuthorizeState authorizeState,
        DiscoveryDocumentResponse disco)
    {
        // Sets the pushed authorization request parameters:
        var challengeBytes = SHA256.HashData(Encoding.UTF8.GetBytes(authorizeState.CodeVerifier));
        var codeChallenge = WebEncoders.Base64UrlEncode(challengeBytes);
        var requestObject = BuildRequestObject(disco);

		var scopes = useKjernejournalforskriften ? ApiScopesKjernejournalforskriften : ApiScopes; 

        var dictionary = new Dictionary<string, string>
        {
            { "client_id", ClientId },
            { "client_assertion", clientAssertionPayload.Value },
            { "client_assertion_type", clientAssertionPayload.Type },
            { "request", requestObject },
            { "scope", scopes + " " + IdentityScopes },
            { "redirect_uri", RedirectUrl},
            { "response_type", OidcConstants.ResponseTypes.Code },
            { "code_challenge", codeChallenge },
            { "code_challenge_method", OidcConstants.CodeChallengeMethods.Sha256 },
            { "state", authorizeState.State },
        };
        return new Parameters(dictionary);
    }

	private static string BuildRequestObject(DiscoveryDocumentResponse disco)
	{
		/* practioner */

		var identifier = new Dictionary<string, object>
		{
			{"id", "16858399649"},
			{"system", "urn:oid:2.16.578.1.12.4.1.4.1"},
		};

		var practioner_authorization = new Dictionary<string, object>
		{            
            //{"code", "AA"},
            {"code", "LE"},
			{"system", "urn:oid:2.16.578.1.12.4.1.1.9060"},
		};

		var legal_entity = new Dictionary<string, object>
		{
			{"id", "981275721"},    // STIFTELSEN BETANIEN HOSPITAL SKIEN
            //{"id", "994598759"},    // NORSK HELSENETT SF
            {"system", "urn:oid:2.16.578.1.12.4.1.4.101"},
		};

		var point_of_care = new Dictionary<string, object>
		{
			{"id", "873255102"},    // Betanien Hospital
            //{"id", "981275721"},    // STIFTELSEN BETANIEN HOSPITAL SKIEN
            //{"id", "994598759"},    // NORSK HELSENETT SF
			{"system", "urn:oid:2.16.578.1.12.4.1.4.101"},
		};

		var department = new Dictionary<string, object>
		{
			{"id", "121313"},    // Avdeling ved Sykehus
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
            //{"code", "010"}, // 
			//{"system", "urn:oid:2.16.578.1.12.4.1.1.8451"},
            {"code", "01"}, // 
			{"system", "urn:oid:2.16.578.1.12.4.1.1.8666"},			
		};

		var purpose_of_use = new Dictionary<string, object>
		{
			{"code", "TREAT"},
			{"system", "urn:oid:2.16.840.1.113883.1.11.20448"},
			//{"code", "1"},
			//{"system", "urn:oid:1.0.14265.1"},			
		};

		var purpose_of_use_details = new Dictionary<string, object>
		{
			{"code", "BEHANDLER"},
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
			{"id", "873255102"}, // BETANIEN HOSPITAL
			{"system", "urn:oid:2.16.578.1.12.4.1.4.101"},
		};

		var patient_department = new Dictionary<string, object>
		{
			//{"id", "4206043"},
			//{"system", "urn:oid:2.16.578.1.12.4.1.4.102"},
			{"id", "121313"},
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

		var serialized = JsonSerializer.Serialize(authorizationDetails);

		var claims = new List<Claim>
		{
			new Claim("authorization_details", serialized, "json"),
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
        var credentials = new JwtSecurityToken(ClientId, disco.Issuer, extraClaims, DateTime.UtcNow,
            DateTime.UtcNow.AddSeconds(60), GetClientAssertionSigningCredentials());

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(credentials);
    }

    private static SigningCredentials GetClientAssertionSigningCredentials()
    {
        var securityKey = new JsonWebKey(JwkPrivateKey);
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

	private static async Task CallApi(string accessToken)
	{
		var urlencodedContent = new FormUrlEncodedContent([
		    new("patient.identifier", PatientId),
			new("status", "current")
	    ]);

        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = urlencodedContent
        }; 

		if (useKjernejournalforskriften)
		{
			// Only send access basis if using scope 'nhn:phr/mhd/read-documentreferences/kjernejournalforskriften'

			request.Headers.Add("hit-access-basis", "SAMTYKKE");
		}

		var proofRequest = new DPoPProofRequest
		{
			Method = request.Method.ToString(),
			Url = request.GetDPoPUrl(),
			// This binds the access token to the DPoP proof:
			AccessToken = accessToken,
		};

		// Set the DPoP proof, we use the same key for this as for the client assertion:
		var proof = new DPoPProofTokenFactory(JwkPrivateKey).CreateProofToken(proofRequest);

		request.SetDPoPToken(accessToken, proof.ProofToken);

		var handler = new HttpClientHandler
		{
			// Handle staging certificate from Let's Encrypt used by PHR API
			ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
		};

		var httpClient = new HttpClient(handler);
		var response = await httpClient.SendAsync(request);

		var responseBody = await response.Content.ReadAsStringAsync();

		Console.WriteLine("Response from the API:");
		Console.WriteLine(response);
		Console.WriteLine("Response body from the API:");
		Console.WriteLine(responseBody);
	}
}
