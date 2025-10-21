using ConsoleUploadDocument;
using ConsoleUploadDocument.Kodeverk;
using ConsoleUploadDocument.Services;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Browser;
using Duende.IdentityModel.OidcClient.DPoP;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NorskHelsenettUpload;
using NorskHelsenettUpload.FluidExtensions;
using NorskHelsenettUpload.Ihe.Mhd.Interfaces;
using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Ihe.Mhd.Services;
using NorskHelsenettUpload.Models;
using PatientHealthRecords.ApiService;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/*
	Configure host and DI
*/

var host = Host.CreateDefaultBuilder(args)
	.ConfigureAppConfiguration((context, config) =>
	{
		config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
	})
	.ConfigureServices((context, services) =>
	{
		var configuration = context.Configuration;

		var apiOptions = configuration.GetSection("ApiOptions").Get<PhrApiOptions>()!;
		services.AddSingleton(apiOptions);
		services.AddSingleton(apiOptions.Environments);

		services.AddScoped<IPhrApiEnvironmentProvider, ConsolePhrApiEnvironmentProvider>();

		services.AddOptions<HelseIdOptions>()
			.BindConfiguration(HelseIdOptions.SectionKey);

		services.AddScoped<IPhrApiCaller, PhrApiCaller>();
		services.AddScoped<IPhrApiService, PhrApiService>();
		services.AddScoped<IBundleCreator, BundleCreator>();

		services.AddHttpClient();		
	})
	.Build();

// Create a scope to resolve scoped services
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

/*
	Configure constants
*/
var patientId = "21825297143";

var firstName = "PUSLETE";
var middleName = "";
var lastName = "PIKE";
var gender = "M";
var dateOfBirth = new DateTime(1952, 2, 21);

var authorNin = "05898597468";
var authorFirstName = "GRØNN";
var authorMiddleName = "";
var authorLastName = "VITS";
var authorRole = "LE";

var organizationName = "STIFTELSEN BETANIEN HOSPITAL SKIEN";
var organizationIdentifier = "981275721";
var organizationAssignOid = "2.16.578.1.12.4.1.4.101";

var ParentOrg = organizationIdentifier;
var ChildOrg = organizationIdentifier;  // TODO: fix
var PatientPointOfCareOrg = organizationIdentifier;

//var hl7Organization = _organizationService.GetHl7OrganizationDetailFromOrganization(sessionData.OrgRegOrganization);

var homeCommunityId = "2.16.578.1.12.4.5.100.15";  // Bjarne sykehus, test environment


/*
	Log in with HelseID. We need a user token (logged in user) to call the bundle endpoint. 
*/
//private const int LocalhostPort = 8080;
const int LocalhostPort = 8089;
string StsUrl = "https://helseid-sts.test.nhn.no";
string RedirectUrl = $"https://localhost:{LocalhostPort.ToString()}/callback";
//string ApiScopes = "nhn:phr/mhd/create-documents-with-reference nhn:phr/mhd/generate-metadata nhn:phr/delete-documents-and-reference";
string ApiScopes = "nhn:phr/mhd/create-documents-with-reference";
//string ApiScopes = "nhn:phr/mhd/generate-metadata nhn:phr/delete-documents-and-reference";
//string ApiScopes = "nhn:phr/mhd/read-documentreferences/kjernejournalforskriften nhn:phr/mhd/read-document nhn:phr/delete-documents-and-reference nhn:phr/mhd/create-documents-with-reference nhn:phr/mhd/generate-metadata"; 
string IdentityScopes = "openid profile offline_access helseid://scopes/identity/pid helseid://scopes/identity/security_level";

var helseIdOptions = services
	.GetRequiredService<IOptions<HelseIdOptions>>()
	.Value;

#region Log in with HelseID

var accessToken = string.Empty;
var identityToken = string.Empty;

try
{	
	using var httpClient = new HttpClient();

	// Download the HelseID metadata from https://helseid-sts.test.nhn.no/.well-known/openid-configuration to determine endpoints and public keys used by HelseID:
	// In a production environment, this document must be cached for better efficiency (both for this client and for HelseID)
	var disco = await httpClient.GetDiscoveryDocumentAsync(StsUrl);

	var options = new OidcClientOptions
	{
		Authority = StsUrl,
		ClientId = helseIdOptions.ClientId,
		RedirectUri = RedirectUrl,
		LoadProfile = false,
		// This validates the identity token (important!):
		//IdentityTokenValidator = new JwtHandlerIdentityTokenValidator()
	};

	// Set the DPoP proof, we can use the same key for this as for the client assertion:
	options.ConfigureDPoP(helseIdOptions.PrivateJwk);

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

	var startUrl = $"{disco.AuthorizeEndpoint}?client_id={helseIdOptions.ClientId}&request_uri={pushedAuthorizationResponse.RequestUri}";

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

	accessToken = refreshTokenResult.AccessToken;		
}
catch (Exception ex)
{
	await Console.Error.WriteLineAsync("Error:");
	await Console.Error.WriteLineAsync(ex.ToString());
}

#endregion

/*
	Configure bundle request (document submission set) 
*/

var currentDirectory = Directory.GetCurrentDirectory();

//var fileName = "Epikrise.xml";
var fileName = "ClinicalDocument_PUSLETE_PIKE.xml";
var filePath = Path.Combine(currentDirectory, @"Examples\", fileName); 

var content = File.ReadAllBytes(filePath);

var fileInfo = new FileInfo(filePath);

var mimeType = GetMimeType(fileName);

var formatCode = CompositionFormatDetector.DetermineFormat(content, ref mimeType, fileName);

var request = new ProvideDocumentBundleRequest();

request.Submission
	.WithUniqueId(Guid.NewGuid())
	.WithTitle($"SubmissionSet-{patientId}-{DateTime.Now}")
	.WithAvailabilityStatus(DocumentSubmissionMetadata.AvailabilityStatusApproved)
	.WithSubmissionTime(DateTime.Now)
	.WithSourceId(homeCommunityId)
	.WithPatientId(patientId)
	.WithAuthor(new Author()
	{
		Name = new NorskHelsenettUpload.Models.Xcn()
		{
			PersonIdentifier = authorNin,
			GivenName = authorFirstName,
			FamilyName = authorLastName
			//MiddleName = sessionData.Name,
			//Prefix
			//Suffix	
			//Degree
		}
	}
	)
	.WithComments("Submission comment")
	;

request.Patient = new SourcePatientInfo() 
	.WithNIN(patientId, NinType.NinTypeNationalIdentityNumber)
	.WithName(new PersonName() 
	{ 
		FirstName = firstName, 
		FamilyName = lastName 
	})
	.WithXdsGlobalIdentifier("1.2.3")
	.WithXdsHomeCommunity("1.2.3")
	.WithGender(gender)
	.WithDateOfBirth(dateOfBirth);

Document document = new Document()
			.WithFilename(fileName)
			.WithTitle("Mikrobiologiske Undersøkelser")
			.WithContent(content, computeSha: true)
			.WithMimeType(mimeType)
			.WithCreationTime(fileInfo.CreationTime)
			.WithSize((int)fileInfo.Length);

var classCode = GetRandomCoding(ClassCodeList.ClassCodes);
var typeCode = GetRandomCoding(TypeCodeList.TypeCodes);
var healthcareFacilityCode = GetRandomCoding(HealthcareFacilityTypeList.HealthcareFacilityTypes);
var practiceSettingCode = GetRandomCoding(PracticeSettingCodeList.PracticeSettingCodes);

var confidentialityCode = ConfidentialityCodeList.ConfidentialityCodes.First(x => x.Code == "N"); 

document.Metadata
			.WithUniqueId(Guid.NewGuid())
			.WithPatientId(patientId)
			.WithSourcePatientIdToHl7String(patientId, NinType.NinTypeNationalIdentityNumber)
			.WithFormatCode(formatCode)
			.WithServiceStartTime(DateTime.Now.Subtract(TimeSpan.FromDays(7)))
			.WithServiceStopTime(DateTime.Now.Subtract(TimeSpan.FromDays(6)))
			.WithClassCode(classCode)
			.WithTypeCode(typeCode)
			.WithConfidentialityCode(confidentialityCode)
			.WithHealthCareFacilityType(healthcareFacilityCode)
			.WithPracticeSettingCode(practiceSettingCode)
			.WithComment("Console upload document")
			.WithAuthor(new Author()
			{
				Name = new Xcn()
				{
					GivenName = authorFirstName,
					FamilyName = authorLastName,
					MiddleName = authorMiddleName
				},
				InstitutionName = [
						new Xon()
						{
							Name = organizationName,
							Identifier = organizationIdentifier,
							AssigningAuthority = $"{organizationAssignOid}"
						}]
			})
		;

/*
document.Metadata.WithAuthor();
document.Metadata.WithEventCode();
document.Metadata.WithFormatCode();
document.Metadata.WithLegalAuthenticator();
document.Metadata.WithSourcePatientId();
document.Metadata.WithSourcePatientInfo();
*/

request.DocumentCollection.WithDocument(document, false); // You can add multiple documents to collection
					
BundleCreator bundleCreator = new BundleCreator();

var bundleDocument = bundleCreator.CreateFhirBundle(request, BundleCreator.COMPREHENSIVE);

/*
	Call PHR API, provide document bundle endpoint
*/
var _phrApiService = services.GetRequiredService<IPhrApiService>();

var apiResult = await _phrApiService.ProvideDocumentBundleAsync(accessToken, helseIdOptions.PrivateJwk, bundleDocument);

if (apiResult.IsSuccess)
{
	Console.WriteLine("Document bundle was successfully uploaded."); 
}
else
{
	Console.WriteLine("ERROR: Document bundle could not be uploaded: " + apiResult.ErrorMessage);
}

/*
	Utility methods
*/

string GetMimeType(string fileName)
{
	var extension = Path.GetExtension(fileName);

	return extension switch
	{
		".xml" => "application/xml",
		".pdf" => "application/pdf",
		".rtf" => "application/rtf",
		".txt" => "text/plain",
		_ => ""
	};
}

TextValue GetRandomTextValue(List<TextValue> values)
{
	var value = values[Random.Shared.Next(values.Count)];

	return value;
}

SimpleCoding GetRandomCoding(List<SimpleCoding> codings)
{	
	var coding = codings[Random.Shared.Next(codings.Count)];

	return coding;
}

string GetRandomCodingToken(List<SimpleCoding> codings)
{
	var codingsTextValues = codings.Select(c =>
				new TextValue(
					$"{c.System}|{c.Code}|{c.Display}", // Value in select list
					$"{c.Display} [{c.Code}]"           // What the user sees
				)).ToList();

	var token = codingsTextValues[Random.Shared.Next(codingsTextValues.Count)].Value;

	return token; 
}

/*
	HelseId helper methods
 */

#region HelseId helper methods
async Task<PushedAuthorizationResponse> GetPushedAuthorizationResponse(
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
Parameters GetParametersForPushedAuthorizationRequest(
	ClientAssertion clientAssertionPayload,
	AuthorizeState authorizeState,
	DiscoveryDocumentResponse disco)
{
	// Sets the pushed authorization request parameters:
	var challengeBytes = SHA256.HashData(Encoding.UTF8.GetBytes(authorizeState.CodeVerifier));
	var codeChallenge = WebEncoders.Base64UrlEncode(challengeBytes);
	var requestObject = BuildRequestObject(disco);

	var dictionary = new Dictionary<string, string>
		{
			{ "client_id",  helseIdOptions.ClientId},
			{ "client_assertion", clientAssertionPayload.Value },
			{ "client_assertion_type", clientAssertionPayload.Type },
			{ "request", requestObject },
			{ "scope", ApiScopes + " " + IdentityScopes },
			{ "redirect_uri", RedirectUrl},
			{ "response_type", OidcConstants.ResponseTypes.Code },
			{ "code_challenge", codeChallenge },
			{ "code_challenge_method", OidcConstants.CodeChallengeMethods.Sha256 },
			{ "state", authorizeState.State },
		};
	return new Parameters(dictionary);
}

string BuildRequestObject(DiscoveryDocumentResponse disco)
{
	/* helseid_authorization */
	var orgNumberDetails = new Dictionary<string, string>
		{
			{"system", "urn:oid:1.0.6523"},
			{"type", "ENH"},
			{
                // The client configuration in HelseID contains a delegation from the supplier of this system to the
                // requested organization number. If this delegation is not set, the request will be invalidated.
                "value", $"NO:ORGNR:{ParentOrg}:{ChildOrg}"
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

	var identifier = new Dictionary<string, object>
		{
			{"id", authorNin},
			{"system", "urn:oid:2.16.578.1.12.4.1.4.1"},
		};

	var practioner_authorization = new Dictionary<string, object>
		{                        
            {"code", authorRole},
			{"system", "urn:oid:2.16.578.1.12.4.1.1.9060"},
		};

	var legal_entity = new Dictionary<string, object>
		{
			{"id", ParentOrg},    
            //{"id", "994598759"},    // NORSK HELSENETT SF
            {"system", "urn:oid:2.16.578.1.12.4.1.4.101"},
		};

	var point_of_care = new Dictionary<string, object>
		{
			{"id", ChildOrg},    
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
			{"id", PatientPointOfCareOrg}, // BETANIEN HOSPITAL
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

	var tillitsrammeverkDetails = new Dictionary<string, object>
		{
			{"type", "nhn:tillitsrammeverk:parameters"},
			{"practitioner", practioner},
			//{"practitioner_role", organization},
			{"care_relationship", care_relationship},
			{"patients", patients},
		};

	var acp = new Dictionary<string, object>
		{
			{"xua-acp", "2.16.578.1.12.4.1.7.2.1.6"},
		};

	var acpObj = new Dictionary<string, object>
		{
			{"type", "xua-acp"},
			{"value", acp},
		};

	var authorizationDetailsList = new List<Dictionary<string, object>>
		{
			helseIdAuthorizationDetails,
			tillitsrammeverkDetails
		};
	//authorizationDetailsList.Add(acpObj);

	//var serialized = JsonSerializer.Serialize(authorizationDetails);
	var serialized = JsonSerializer.Serialize(authorizationDetailsList);

	var claims = new List<Claim>
		{
			new Claim("authorization_details", serialized, "json"),
			new Claim(JwtClaimTypes.Subject, helseIdOptions.ClientId),
			new Claim(JwtClaimTypes.IssuedAt, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
			new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N")),
		};

	return BuildHelseIdJwt(disco, claims);
}

ClientAssertion GetClientAssertionPayload(DiscoveryDocumentResponse disco)
{
	var clientAssertion = BuildClientAssertion(disco);

	return new ClientAssertion
	{
		Type = OidcConstants.ClientAssertionTypes.JwtBearer,
		Value = clientAssertion,
	};
}

string BuildClientAssertion(DiscoveryDocumentResponse disco)
{
	var claims = new List<Claim>
		{
			new Claim(JwtClaimTypes.Subject, helseIdOptions.ClientId),
			new Claim(JwtClaimTypes.IssuedAt, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
			new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N")),
            //new Claim("xua-acp", "2.16.578.1.12.4.1.7.2.1.6"),
            //new Claim("bppc-docid", "2.16.578.1.12.4.1.7.2.2.1"),
        };

	return BuildHelseIdJwt(disco, claims);
}

string BuildHelseIdJwt(DiscoveryDocumentResponse disco, List<Claim> extraClaims)
{
	var signingCredentials = GetClientAssertionSigningCredentials();

	// Create JWT Payload
	var payload = new Dictionary<string, object>
		{
			{ "iss", helseIdOptions!.ClientId },
			{ "aud", disco.Issuer },
			{ "exp", new DateTimeOffset(DateTime.UtcNow.AddSeconds(60)).ToUnixTimeSeconds() },
			{ "iat", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() },
		};

	// Add extra claims to the payload
	foreach (var claim in extraClaims)
	{
		payload[claim.Type] = claim.Value;
	}

	// Create the token
	var tokenHandler = new JsonWebTokenHandler();
	var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
	{
		Claims = payload,
		SigningCredentials = signingCredentials,
		// 'AdditionalHeaderClaims' is not needed for 'alg', 'kid', etc.
		AdditionalHeaderClaims = new Dictionary<string, object>
			{
				{ "typ", "client-authentication+jwt" }
			}
	});

	return token;
}

SigningCredentials GetClientAssertionSigningCredentials()
{
	var securityKey = new JsonWebKey(helseIdOptions.PrivateJwk);
	return new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
}

LoginResult ValidateIdentityClaims(LoginResult loginResult)
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
#endregion