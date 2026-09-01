using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient.DPoP;
using Hl7.Fhir.Model;
using Hl7.Fhir.Model.CdsHooks;
using Microsoft.IdentityModel.Tokens;
using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Models;
using PatientHealthRecordsEPJ.Models;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PatientHealthRecordsEPJ.Services;

public class KjernejournalLoginApiService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger<KjernejournalLoginApiService> _logger;

	public KjernejournalLoginApiService(IHttpClientFactory httpClientFactory, ILogger<KjernejournalLoginApiService> logger)
	{
		_httpClientFactory = httpClientFactory;
		_logger = logger;
	}

	public async Task<KjernejournalSessionCreated?> CreateSession(SessionData sessionData)
	{
		var httpClientKjLogin = _httpClientFactory.CreateClient("KjernejournalLoginApiClient");		

		var patientId = sessionData.PatientId; 

		var patientIdSystem = sessionData.SourcePatientInfo?.NinType switch
		{
			NinType.NinTypeNationalIdentityNumber => NinSystem.NinSystemNationalIdentityNumber,
			NinType.NinTypeDnumber => NinSystem.NinSystemDnumber,
			_ => ""
		};

		var accessBasisCode = sessionData.GrunnlagId ?? "SAMTYKKE"; 

		var ehrCodeVerifier = CreateEhrCodeVerifier();
		var ehrCodeChallenge = CreateEhrCodeChallengeS256(ehrCodeVerifier);
		sessionData.KjernejournalEhrCodeVerifier = ehrCodeVerifier;

		KjernejournalCreateSessionModel body = new KjernejournalCreateSessionModel()
		{
			claims = new Claims()
			{
				patient_identifier = new Patient_Identifier()
				{
					id = patientId,
					system = patientIdSystem,
					authority = "https://www.skatteetaten.no"
				},
				practitioner_authorization = new Practitioner_Authorization()
				{
					code = sessionData.AuthorizationId,
					system = "urn:oid:2.16.578.1.12.4.1.1.9060",
					assigner = "https://www.helsedirektoratet.no/"
				},
				access_basis = new Access_Basis()
				{
					code = accessBasisCode,
					system = "urn:oid:2.16.578.1.12.4.5.11.1",
					assigner = "https://nhn.no"
				}
			},
			ehr_code_challenge = ehrCodeChallenge
		};

		var jsonBody = JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower}); 
		StringContent bodyContent = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

		var apiUrl = httpClientKjLogin.BaseAddress + $"api/session/create"; 

		HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
		requestMessage.Content = bodyContent;

		var headerKeyValues = new List<KeyValuePair<string, string>>()
		{
			//new KeyValuePair<string, string>("nhn-patient-nin", patientId),
			new KeyValuePair<string, string>("nhn-source-system", patientId),
			new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()),
			//new KeyValuePair<string, string>("nhn-access-basis", "UNNTAK")

		};

		foreach (var keyValue in headerKeyValues)
		{
			requestMessage.Headers.Add(keyValue.Key, keyValue.Value);
		}

		var proof = HelseIdDpopProofFactory.Create(Constants.JwkPrivateKey, requestMessage.RequestUri!.ToString(), requestMessage.Method.ToString(), accessToken: sessionData.KjernejournalAccessToken);

		requestMessage.SetDPoPToken(sessionData.KjernejournalAccessToken, proof);

		var httpClient = new HttpClient();

		try
		{
			var response = await httpClient.SendAsync(requestMessage);

			var content = await response.Content.ReadAsStringAsync();
			var kjernejournalSession = JsonSerializer.Deserialize<KjernejournalSessionCreated>(content);

			return kjernejournalSession;
		}
		catch (Exception ex)
		{
			_logger.LogError($"Could not create Kjernejournal session {apiUrl}", ex);
		}

		return null;
	}

	private static string CreateEhrCodeVerifier()
	{
		//var bytes = RandomNumberGenerator.GetBytes(32);
		var bytes = RandomNumberGenerator.GetBytes(64);
		return Base64UrlEncoder.Encode(bytes);
	}

	private static string CreateEhrCodeChallengeS256(string codeVerifier)
	{
		var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
		return Base64UrlEncoder.Encode(hash);
	}

	public Hl7.Fhir.Model.Organization GetHl7OrganizationDetailFromOrganization(OrgRegOrganization orgRegOrganization)
	{
		var organization = new Organization();

		if (orgRegOrganization != null)
		{
			organization.Name = orgRegOrganization.name;
			organization.Identifier = [

				new Hl7.Fhir.Model.Identifier()
				{
					Value = orgRegOrganization.id,
					System = "2.16.578.1.12.4.1.4.101"
				}];
		}

		return organization;
	}
}
