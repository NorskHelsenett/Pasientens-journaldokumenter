using PatientHealthRecords.ApiService;
using System.Text;

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

	public async Task<ApiResult> ProvideDocumentBundleAsync(string content, bool validate, PhrApiEnvironment? apiEnvironment = null)
    {
		const long MaxRequestBodySize = 100 * 1024 * 1024;

		var headerKeyValues = new List<KeyValuePair<string, string>>();

		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-source-system", "PJD Test EPJ"));
		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()));

		StringContent stringContent = new StringContent(content, Encoding.UTF8, mediaType: "application/fhir+json");

		apiEnvironment ??= _phrApiEnvironmentProvider.GetCurrentEnvironment();

		var apiUrl = apiEnvironment.ProvideDocumentBundleUrl;

		if (validate)
		{
			apiUrl += "/$validate";
		}

		//var host = _httpClient.BaseAddress;
		//var getByNinUrl = $"{host}{GetByNinPath}?{informationPartsQueryString}";				

		//var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.pjd.test.nhn.no/R4/fhir/Bundle");
		//var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:8443/R4/fhir/Bundle");
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
		
		requestMessage.Content = stringContent;
		
		var contentBytes = await stringContent.ReadAsByteArrayAsync(); 

		var contentLength = contentBytes?.Length;

		if (contentLength.HasValue && contentLength.Value > MaxRequestBodySize)
		{
			return new ApiResult()
			{
				IsSuccess = false,
				StatusCode = 413, // Payload Too Large
				ErrorMessage = $"Request body size {contentLength.Value} ({contentLength.ToPrettySize()})  bytes exceeds the maximum allowed size of {MaxRequestBodySize} ({((int?)MaxRequestBodySize).ToPrettySize()}) bytes. The size of a document in the request will be about 33% larger than on disk due to base64 encoding.",
				IsPayloadTooLarge = true,
				PayloadSizeInBytes= contentLength.Value,
			};
		}

		foreach (var headerKeyValue in headerKeyValues)
		{
			requestMessage.Headers.Add(headerKeyValue.Key, headerKeyValue.Value);
		}

		// Expect: 100-continue is meant for HTTP/1.1 and is not supported in HTTP/2, which is used by .NET HttpClient. Setting this header will not have the intended effect and may lead to unexpected behavior. Since .NET HttpClient does not support Expect: 100-continue, we should not set this header.
		// requestMessage.Headers.ExpectContinue = true; 

		// Note: DPoP and access token is handled through HelseIdDelegatingHandler in HelseId.cs

		try
		{
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
		catch (HttpRequestException ex)
		{
			// This can happen if the server/proxy closes the connection before the client receives the 413 (payload too large) response.

			return new ApiResult()
			{
				IsSuccess = false,
				StatusCode = 0,
				ErrorMessage = ex.Message
			};
		}
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
