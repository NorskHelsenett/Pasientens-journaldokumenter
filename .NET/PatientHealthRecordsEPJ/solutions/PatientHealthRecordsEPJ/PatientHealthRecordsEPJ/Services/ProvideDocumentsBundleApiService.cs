using Hl7.Fhir.Model.CdsHooks;
using NorskHelsenettUpload.Models;
using PatientHealthRecords.ApiService;
using System;
using System.Text;
using System.Text.Json;

namespace PatientHealthRecordsEPJ.Services;

public class ProvideBundle
{        
	private const string ProvideDocumentBundlePath = "/R4/fhir/Bundle";	

	private readonly HttpClient _httpClient;

	private readonly IPhrApiEnvironmentProvider _phrApiEnvironmentProvider;

	public ProvideBundle(HttpClient httpClient, IPhrApiEnvironmentProvider phrApiEnvironmentProvider)
    {        
        _httpClient = httpClient;                
		_phrApiEnvironmentProvider = phrApiEnvironmentProvider;
	}

	public async Task<ApiResult> ProvideDocumentBundleAsync(string content)
    {
		var headerKeyValues = new List<KeyValuePair<string, string>>();

		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-source-system", "PJD Test EPJ"));
		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()));

		StringContent stringContent = new StringContent(content, Encoding.UTF8, mediaType: "application/fhir+json");

		var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

		var apiUrl = _apiEnvironment.ProvideDocumentBundleUrl;

		//var host = _httpClient.BaseAddress;
		//var getByNinUrl = $"{host}{GetByNinPath}?{informationPartsQueryString}";				

		//var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.pjd.test.nhn.no/R4/fhir/Bundle");
		//var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:8443/R4/fhir/Bundle");
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);

		requestMessage.Content = stringContent;

		// Note: DPoP and access token is handled through HelseIdDelegatingHandler in HelseId.cs

		var response = await _httpClient.SendAsync(requestMessage);

		var responseBody = await response.Content.ReadAsStringAsync();

		return new ApiResult()
		{
			IsSuccess = response.StatusCode == System.Net.HttpStatusCode.OK,
			StatusCode = (int)response.StatusCode,
			Body = responseBody,
			ContentType = response.Content.Headers.ContentType?.ToString()
		};		
    }
}

public class ProvideDocumentBundleApiService
{
    //private readonly IDPoPProofCreator _idPoPProofCreator;
        
    public ProvideDocumentBundleApiService(IHttpClientFactory factory /*IDPoPProofCreator idPoPProofCreator*/, IPhrApiEnvironmentProvider phrApiEnvironmentProvider)   
    {
		// httpClient is configured with DI in Startup.cs with builder.Services.AddHttpClient<"CreateDocumentsWithReferenceApiClient">(client => ...)
		var httpClient = factory.CreateClient("ProvideDocumentBundleApiClient");

		//_idPoPProofCreator = idPoPProofCreator;

		//FullAccess = new FullAccess(httpClient, idPoPProofCreator);
		ProvideBundle = new ProvideBundle(httpClient, phrApiEnvironmentProvider);		
    }

    public ProvideBundle ProvideBundle { get; }   
}
