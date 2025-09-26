using NorskHelsenettUpload.Models;
using System.Text.Json;

namespace PJD.Services.Persontjenesten;

public class FullAccess
{        
	private const string GetByNinPath = "/v3/full-access/person/get-by-nin";	

	private readonly HttpClient _httpClient;

    public FullAccess(HttpClient httpClient)
    {        
        _httpClient = httpClient;                
    }

	public async Task<PersontjenestenPerson?> GetByNin(string nin, string[] informationParts, bool includeHistory = false)
    {       
		PersontjenestenPerson? person = null!;

        var informationPartsQueryString = string.Join("&", informationParts.Select(p => $"informationParts={p}"));

        var host = _httpClient.BaseAddress;
		var getByNinUrl = $"{host}{GetByNinPath}?{informationPartsQueryString}";
		
		var formUrlEncodedContent = new FormUrlEncodedContent([
            new("nin", nin),
            ]);
               		
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, getByNinUrl);
				
        requestMessage.Content = formUrlEncodedContent;

		// Note: DPoP and access token is handled through HelseIdDelegatingHandler in HelseId.cs

		var _response = await _httpClient.SendAsync(requestMessage);

        if (_response.IsSuccessStatusCode)
        {
            var responseBody = await _response.Content.ReadAsStringAsync();

            person = JsonSerializer.Deserialize<PersontjenestenPerson>(responseBody); 
        }

        return person; 
    }
}

public class RestrictedAccess
{

}

public class PersonApiService
{
    //private readonly IDPoPProofCreator _idPoPProofCreator;
        
    public PersonApiService(IHttpClientFactory factory /*IDPoPProofCreator idPoPProofCreator*/)   
    {
        // httpClient is configured with DI in Startup.cs with builder.Services.AddHttpClient<"PersonApiClient">(client => ...)
        var httpClient = factory.CreateClient("PersonApiClient");
        
        //_idPoPProofCreator = idPoPProofCreator;

        //FullAccess = new FullAccess(httpClient, idPoPProofCreator);
		FullAccess = new FullAccess(httpClient);
		RestrictedAccess = new RestrictedAccess();
    }

    public FullAccess FullAccess { get; }
    public RestrictedAccess RestrictedAccess { get; }
}
