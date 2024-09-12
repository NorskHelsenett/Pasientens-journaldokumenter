using IdentityModel;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.DPoP;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.OidcClient.Browser;
using Microsoft.AspNetCore.WebUtilities;

namespace PasientensJournalDokumenter.Samples.NativeClientWithUserLoginAndApiCall
{
	class Program
	{
		//
		// The values below will normally be configurable
		//

		// In a production environment, you MUST use your own private key stored in a safe environment (a vault, a certificate store, a secure database or similar).
		// The library expects an 'alg' property in the JWK key:
		/*private const string JwkPrivateKey = """
                                             {
                                               "alg": "RS256",
                                               "d":"ah-htj9aYcA-Ycpec0g84Q9dVP2HRW009nxZW38NypaJgciuN0V4FRWz2JtXOUdTMlZVtJETulYmfKKWYqVATi7STeAFMMT0ZhsQVz8D9z4kb9m9-lKQTJkS4lAdRVajXSeDAC_L9PWMqHGa7n3C_wkYU-ov59STncw1Xxfsk-ZQkGqonBDcWs_emJ6Yfw1mo7Uq4gPWa0ZsHj9oGFfH0ExubzNY6HnrFYnXGTxzLIhzfPQB3Zur2JZAveApyChWcI4Ry-H6pmBkL7VWT78eFCG4RlXBH2rNdXVj2yYlx2ILQTXDPLOOOEQm5w2rk5YeiuDEb16geJtr97rEaQylbfc3crjGpiMAmLinzkSFFjUvDq9uKdAPM7xisW0uRLElmiGsrdvQPGjtpjigDlOklYZgxESVrpVdr-UuDfe-jGiwCINZpbZ5iyvk8X7kpbsjN1tFnpLW7ojOAHKsQCoQiDZUiGQ9eG4SYqsgH_dd1Y67Kl0DeTbOhlGnlTqTCIHuCVXpZT9WXGdDEiluUhtRNApfzoNjzGDDg5gahrjMUN69bCxxVdmGnqQRN4-e4NxkRxbP5dgRkd0f9Eh80p7Aio6-qkUb3U1kY5kjICP5QGCZluQncJLifhIV1Ui0KOZI4t7bK4sgiY1QtrPlHleByR1KsRpQYibGiphupOxQDcE",
                                               "dp":"EufIi5gLWC7J9CPEP4sYCR4zkxyImE7e4RlPv3S5yNSvwgsHaqy1WxS19iR0sp1_QzOH8Z-RnzWHsG2jcowSJ3cLHoWA059tQZqNMDhtiEAWMs9Le1ctK85rKf9kwPzADyj54tDGWgiBulxX8UwMyF0SWvtlRIn8gPaTEe1HeSiDFOoQJQSSpsX6GC_80P8uf2qwEvDymcYbNB_TwmPwMkB0s-krP3eRshjuokmb2ImSJCTzXi9pIsGjVGl7yjV8k-MwQqz0jr5sabBvDA0hMNghdPmI7DWM65XPma5xf-_BjZfPQ9FRpDoj5PB9mVU-_IkzSkOJ1_fZZxfiYT1Epw",
                                               "dq":"PU21zTQEjZBfxyS8QzWI6XthzGxdxDdj_293lCtKvTNkm6B_24H-X3yhwKu-99MJR6Khx2UWJVI5BbRvGAhvnp_OeGsy67H_dv59Lq_57ZiYBTCp1_VsE7AyWz_OfSdyS6cVeoWUsxZ1zZJu3j4RuMkQI90qspGpDuCuenyrfUAogxGjzUVh9u0Q5yMOMycI52Qrp5I5BsaTIf92eEZX0oABuaPmNRtarKNzn93g5WMpamWWbTF5LTcjZuM-gZwViUNl1pjC0xCVTody-N38vT91PcNYfPcYdfW88Wk_45PO-DXFEDPBuVF2g-epy5-5FldlGvLGFUYRMo96vs4N0Q",
                                               "e":"AQAB",
                                               "kty":"RSA",
                                               "n":"uJTJ56hHiQpT7xn7ax6n50xomosd6fac_rRXIDvZzSfQVchzqWMibCV0hShv44JsAnTNvmNpoUbFwMuf8MBtfTjbIGjryUgbQFmCBtgSoYy-VCANnaAxx_jSa9sJmmMyrqdoFTdpzVqP6WCpXOSd0VbL6Pi-I3fe3HPKz7d25Q-TO23abw1Rt3fbUL-nf40HZkNHW_Skro7_RVLEe3znKVYxwdU5saSad0hf5gssFiVlBgeZ7-Ua_NygJnzYYRZ-DKPxZHgN-JMKb7Rh3nJzk0szCY15yNvjhWZ2mcbT2Vqtg92t8c5JkJf2uj4WnoDUaoauQT5lQN5w9z2cBHbN22lOdYs92PcWwp6giLJxTwYEarytx2f84tv0JuJA2M6d4oKQSBvgTlZfOPc_Litg4T9EeJ0cs0PUHpegaG-b4UaD3786UkC7caea_1h_-ieFhkMBWz0YnJY40TGD1HhCqu-fmL5_QiuMUH_w3_A3_ZiLIoyPfTe3eIhMC4biGuoVQq-88Z9e5oqk_NgZ4ibfL1WNVp5EZWL-2lnZpqZi0qE7WpX_7_sWqAErIttv-161UzYO_8GIsVmE1CNBe8TGQfTwQ3xRP5pNIiPVcdumPIUgVmNk4SCLCjio5SzunGB7NESGDOXVOWCHUZ5FL998bLD5kTdJThoXQvCn6FGl3tE",
                                               "p":"2UEy1LV7aLsxGuAw_MiLKhJBgbAsvzOhX5OWRtsFxU6JZFyJQZaDVbIHwld7Fn96cVWncQ16REhPoW8vAbqiNO6-fOh-dP3ACPgXG3_gbcfRzg-iZAqd32R1oICBS-Jro5Nqmehw4ufiUpksd_EwuQZFlG-DvsnGw8NP4SHdL6BO43USf20cBqv2ZrV8KUwnQnGLoceZartovE8tqVnwNS1s3xdCazsUrb9Nqs_uJBB1r23SFcLUx5wU65y3IcTIQ9x0b2DhU9nlhU_60I-FF_wnMUmRYgNfWX70FOuJ9btjITvgBcWe1y-42uT00JczB1NCubJY8nmHvZ9gPy_7Kw",
                                               "q":"2X_gcTlYKfoyqUA9wQzrCCcuyN82cgvduYjM8Gg0AU33j20lFhbBKC1AmaHBS_lc8k5nsSfEt7ewgT1gvbFjU7MD_pTyA9Z3pJ1Fsst0tocePKfYNMOhqiJ0IRHDbC5WFBYwlWKNl0Vq0v1mkIhex0XcepXgglVPR6V-NuyuNEVDV9bZ9NGI37kmwIartq0gBQ87Qbd_vJXQAEH5dHsRTrc3dUSgcY2UH3YUqvuvxmdQkauHMI9VPcJdU1sZdOS2yeh7pfQoE8g8vqb95dkNIKdy7yfjnxfpkDeyfV6XLvMQPic12t6nmH25j41T-XrMh32NnTZzo3sLigsMufDf8w",
                                               "qi":"BZRvnc35ZRiwl7jX8qOVnbdN9x7W1jPNJ8SWIpVFVAPYhhgYkAPuUwj5pmpcCVghba1PFpBqNXNf3xzX-G2WZ7oNSXUIyYeW0zGrxUbk568xE9sxshe6Ac8-nreHuzMY2xcaV6ryFqH0HsDAiRLZvqKljb3ZjUx5NNypzRRwQZCo7_2eK1ArwqRc1F1VYtxFc_MjfoaxJ2CzUEKXBqmp2eAdf14gGeBTi_MDPh3PwYH-qhnRGS6QWCAPkUNGWJr6q-hNiwgCiIqRzBQ4hufa3vDfxNiaSDoYs-U1Nd57gduzYKPzPtVEsDBIn7d6A_Mo-ULFuAdwqPl3Xj7iJRUP6w",
                                               "kid":"5DC654C985D1037A16D82FCA3B9B8843"
                                             }
                                             """;*/

		// This client_id is only to be used for this particular sample. Your application will use its own client_id.
		//private const string ClientId = "f4352589-549d-47ec-9844-5255f4eb0fad";

		private const string JwkPrivateKey = """
		{
			"alg": "RS256",
			"d": "H2DLX0rxRTXfqxfbsSlgBxTm-mqugSbmTlqWPq8GAqx_WWRN7VDmrX89q8LCgSUQJUjincdVf_5LoaWB3CeAovMMr5K8nCvh2SZTO-YOcQPerX3XcFISLsCTPhGjgNJxbmn3nTpk7k9-Km6Iu4VleljPktM9ft3M2znKkhlJN69uXu0kx908oVz3f4eqCS70zz-qdRq6Al7jhmRnRtPCVSxbno-aoO26S_TqQR9bbBs4g7kLIDuSiDptoBM3EGvs6fogbgLTBNUT--1jpxb5N7tumBg-wWk47SCCn-KE7yvpoOypCzftjYPZNIKUK4IqMCBJOLlsEEgjHr9T0w7eYLFxt9KlypG2CX2iet7yhKSNXGkP9_HVYY7AJpnVYFRJdxcJS0KjGwf3N_OE_4uuHEIPesodoc5D62ofEgygYeH4ILaA7nHrQ3SnqNN5FQ7P0RRKh7y99moRFDbK7zS5N71pb2-rUrQroOpoh0A4RDQ95DvQQZkfrMsX-8qS2dtNZCb7QmaEfkzIvAZAaXJhMus4rN0FDIs3USoqPa7gnAKhJlsJmmYZyCRd5u-ijvmshAqQ84AisqAXGy-aSy0G6ksjXNJ98S_V70_cbEufar2tHwsluy2oGtHjQUUPY2UbRM_Rreps46QLXClA69EEntpJ3gW1yXxgYBihazMBB2E",
			"dp": "qIL93nZ2kwlSbWXahB28y8lfdAbcCevVGLCZGACLPyqZgfjy719E4br309zmVx1KjN6arWSFjBPpM531NWH_o9NcWGCeE7dV8HEtPWtQg71rO3iN1s0ISxZqDO4nCYfeZPgJqM7l2LF0GlrOVDg0K2BdD3nhhrXsKcLKe41yuyUD3J2NfcF6DKuduS7OlnDwptD6Kw1eOJNgp9a4Q66CyE3Gh6QmxYNw2S-iDb-m5lvmk3gcJEcUZnIXMyzU43Yn2TwZs00sgaOvxnuJcoV9r7rtQs19FU7cPiRRKr1tJh4Z_uEhV_NDRyJZa441xK_8uy5ozecxPKuROV2GF6e0Bw",
			"dq": "Ic-aAapb4l1KWnvzIEUSMyqS01FvDymcBY0wDZUF1wr3SBrmVsDzlgl3mbzjuGsg3xETJ8nMBAufuuuexeuvNbk0mBhH0WRthTi9twGOLQyEYayDcoT5qdtYjaNu6e6Pb8dePKj7hLnN06KRjnuIpiK-OdRwIJ9KDbgMQWqhIDkJEQ2tzSD_92SA9qxfSnQ5Qh4Edg6lgcU-dPZG_0oW_wHfB1VjB-ZzMT85eVzQJ72cCB35naDJS5e5czemAHovk9s7dy14mTcYb3z7Fz-pDgBwKI44Tex_aQyjlo2QUk5_82kHsO2SoxbaOJtxKpFkga9q8cPS18vgfOtMeEToGw",
			"e": "AQAB",
			"kty": "RSA",
			"n": "xs3HYJO2v28xUr9s61Ynm_CCQpDAvQPyRVF8w75iPqBr2hOVhZ0asFtZ9hx3rp2hngVX38aqDlliANH0jsiufThq-qPXL-zxq9vgYHPhr0cSDPssGylxEnsy2YNOREFkZtcqWYpypzZcHNMigEAsV91mQJ02GmoELuPUvJTFngLr9mKwuvOjTnF4F7zroMqw9TiqyOOqZZc2LoaTL5ewO8KkmerYN6TOi11oF_8fXjWSSbDGnV6CXvWmxNJw8Z7rW3J_T3lkkeugmBb001HG8nQtn_JPb0T1RIowtxG9hHnIfBjMH7PLEtf5eai0H3BmnXZuThK6vvcst8tmBgGelduxbD6eXeVKA19KqOu3Z4q8-78oemnQTn7w9eeP6VJrFxTFuZEt2-8Qhwv7Jf-l0hGmd6SR3SnCdb9-UVfDtE77N90n8B1ovI0f4ncwqK4-ARUl2qWYMw3oeZWbqJixpPbfwl-_86352UmRMyyIyX9Pm3Fn8RTRX0X21uni8oZoYDbDh0GFguzTpkI8BTbkzpUjJiGAfQbe-lrMjHOyqlA3srzl6B6DDVm-LLWCa6-tMMY09ShTsZvP6KFHqP-DDxcks_ydwGqMuPG7nKk2NzlwXHbC32rifCgmufrcFU7l6_2mXPtigGyjrYx60cA2X29Xt0lJsWQPA1P_j5FaQC0",
			"p": "8byloLrI3-s650MAa4um9M-CBEhr0UPtQ7e5Q35C8FkrCH3zjPWeM1bY-cRsfE7XM39ze_9SVj9WxNlyQlSyyYK9AfMou_9FBKbRLJRk1j9NuJ7KaMUEY2aDSdbyqtMtLNvxxYkHO2CX8iRNy3a218CdWRApJ_TDm7wV-q0bkT_NcT1as5EddXo1_U1SmSp26wMxp1CjHsReLkVcn72CqdkSKHB3Y2F_twWD6R_fCizM4tkuh13-KMwAlwF2gDGN5XMtAqILJP2JaBU0Hot5edHjVJzgoUVgIpxkbvcyNhIIccr1KcF4h6Nuip6Zp8xSCCpLuomyotVoEco8cmm3Lw",
			"q": "0oikdF7M3cHle37VzguW1TwNqrrPZIsboogaJER6yMgvv3N45sJ-y7TuJuFQwo5rH5glYZU3kwYl9Qnwz65mPVpRtVl7eukJdADAoeBcMvMFn458wPDNc-AMrAMgdLBsiaOrjRQ3f4JcUQ3R9S2ujvS_SlKygh00Pv_HzP3lp5C6qDx85OY4SwYQqmQc3gJHhJ2tD2xlrjIP9f8sCdpa2VK5OZUunCQrrUNeN6h5VjguQXpQZv5YG3YQqO0IilqhBQGED0wy2hKr7fAnzE63fBuu-tQRte54oZp9uv_NZ6kTYh5hxxDxTPhSS8hLc0fNrXIdXMMu0Ac035J7mWPnYw",
			"qi": "6e_4oR3QAb8xyiM-8Bp7DipbgCIBc4r25seSfoegufsTYP-3EGJAzWzqhEe9lktUznBOf0NJ1VaVn87wYOyDv9-arPHxxKU2xTWA6j1AyZGzkzqKHKpbtemS8I5-TNoyFARkcyHhCQ4VcQ8JW-3V6kbYgekgxBy081bNsKQx-ye-fPd_vkEoQsJEq8Za1x8DdRWb-Wd_rOVsogYCQ2tZk7LzJI3zH4GWv0d-w1AxrbHBrRTnL_l0NWRRH9tkCC6AKNufw5-YZicuSW9DEmwqS-2M-QfI9NQ5OVOPMugukMPHc8Ohln0r55qJijqAKG2SIZlfdaAHX25DmzVeQP24ag"
		}
		""";


		private const string ClientId = "3fe5151f-595b-4977-ab4f-aec7baf328d1";

		// The client is configured in the HelseID test environment:
		private const string StsUrl = "https://helseid-sts.test.nhn.no";

		// An API that requires a logged in user
		// You can find README for this API in the SampleApi folder in this repository
		//private const string ApiUrl = "https://localhost:5081/user-login-clients/greetings";
		private const string ApiUrl = "https://api.pjd.test.nhn.no/fhir/documentreference/_search";

		// The port do not need to be pre-registered in HelseID, which means that you can allocate an available port on your localhost when launching the application:
		private const int LocalhostPort = 8089;

		// In a test environment, the port does not need to be pre-registered in HelseID Selvbetjening;
		// this means that you can allocate any available port when launching the application:
		private static readonly string RedirectUrl = $"http://localhost:{LocalhostPort.ToString()}/callback";

		// This is the scope of the API you want to call (get an access token for)
		//private const string ApiScopes = "nhn:helseid-public-samplecode/authorization-code";
		//private const string ApiScopes = "nhn:phr-public-samplecode/authorization-code";
		private const string ApiScopes = "nhn:phr/mhd/read-documentreferences";

		// These scopes indicate that you want an ID-token ("openid"), and what information about the user you want the ID-token to contain
		private const string IdentityScopes = "openid profile helseid://scopes/identity/pid helseid://scopes/identity/security_level";

		static async Task Main(string[] args)
		{
			try
			{
				using var httpClient = new HttpClient();

				// Setup the oidc client for user authentication against HelseID
				var options = new OidcClientOptions
				{
					Authority = StsUrl,
					ClientId = ClientId,
					RedirectUri = RedirectUrl,
					FilterClaims = false,
					// This validates the identity token (important!):
					IdentityTokenValidator = new JwtHandlerIdentityTokenValidator(),
				};

				// Set the DPoP proof, we can use the same key for this as for the client assertion:
				options.ConfigureDPoP(JwkPrivateKey);

				var oidcClient = new OidcClient(options);

				// Authenticate with HelseID using the request object via the system browser
				// The authorizeState object contains the state that needs to be held between starting the authorize request and the response
				var authorizeState = await oidcClient.PrepareLoginAsync();

				// Download the HelseID metadata from https://helseid-sts.test.nhn.no/.well-known/openid-configuration to determine endpoints and public keys used by HelseID:
				// In a production environment, this document must be cached for better efficiency (both for this client and for HelseID)
				var disco = await httpClient.GetDiscoveryDocumentAsync(StsUrl);

				var pushedAuthorizationResponse = await GetPushedAuthorizationResponse(
					httpClient,
					disco,
					authorizeState);

				if (pushedAuthorizationResponse.IsError)
				{
					throw new Exception($"{pushedAuthorizationResponse.Error}: JSON: {pushedAuthorizationResponse.Json}");
				}

				var urlForAuthorizeEndpoint = $"{disco.AuthorizeEndpoint}?client_id={ClientId}&request_uri={pushedAuthorizationResponse.RequestUri}";

				var browserOptions = new BrowserOptions(urlForAuthorizeEndpoint, RedirectUrl);

				// Create a redirect URI using an available port on the loopback address.,

				var browser = new SystemBrowser(port: LocalhostPort);

				var browserResult = await browser.InvokeAsync(browserOptions, default);

				// We need a new client assertion for the call to the /token endpoint
				oidcClient.Options.ClientAssertion = GetClientAssertionPayload(disco);

				// If the result type is success, the browser result should contain the authorization code.
				// We can now call the /token endpoint with the authorization code in order to get tokens:
				var loginResult = await oidcClient.ProcessResponseAsync(browserResult.Response, authorizeState);

				if (loginResult.IsError == false)
				{
					loginResult = ValidateIdentityClaims(loginResult);
				}

				if (loginResult.IsError)
				{
					throw new Exception($"{loginResult.Error}: Description: {loginResult.ErrorDescription}");
				}

				Console.WriteLine($"Identity token from login: {loginResult.IdentityToken}");
				Console.WriteLine($"DPoP token from login: {loginResult.AccessToken}");
				// Call the example API
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
			DiscoveryDocumentResponse disco,
			AuthorizeState authorizeState)
		{
			// Sets the pushed authorization request parameters:
			var challengeBytes = SHA256.HashData(Encoding.UTF8.GetBytes(authorizeState.CodeVerifier));
			var codeChallenge = WebEncoders.Base64UrlEncode(challengeBytes);

			// Setup a client assertion - this will authenticate the client (this application)
			var clientAssertionPayload = GetClientAssertionPayload(disco);

			var pushedAuthorizationRequest = new PushedAuthorizationRequest
			{
				Address = disco.PushedAuthorizationRequestEndpoint,
				ClientId = ClientId,
				ClientAssertion = clientAssertionPayload,
				RedirectUri = RedirectUrl,
				Scope = ApiScopes + " " + IdentityScopes,
				ResponseType = OidcConstants.ResponseTypes.Code,
				ClientCredentialStyle = ClientCredentialStyle.PostBody,
				CodeChallenge = codeChallenge,
				CodeChallengeMethod = OidcConstants.CodeChallengeMethods.Sha256,
				State = authorizeState.State,
			};

			// Calls the /par endpoint in order to get a request URI for the /authorize endpoint
			return await httpClient.PushAuthorizationAsync(pushedAuthorizationRequest);
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

			var credentials =
				new JwtSecurityToken(
					ClientId,
					disco.Issuer,
					claims,
					DateTime.UtcNow,
					DateTime.UtcNow.AddSeconds(60),
					GetClientAssertionSigningCredentials());

			var tokenHandler = new JwtSecurityTokenHandler();
			return tokenHandler.WriteToken(credentials);
		}

		private static SigningCredentials GetClientAssertionSigningCredentials()
		{
			var securityKey = new JsonWebKey(JwkPrivateKey);
			return new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
		}

		private static async Task CallApi(string accessToken)
		{
			// We need to use the HttpRequestMessage type for this functionality:
			//var request = new HttpRequestMessage(HttpMethod.Get, ApiUrl);
			var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);

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
