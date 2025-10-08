using Hl7.Fhir.Model;
using PatientHealthRecords.ApiService.Ihe.Mhd.Models;
using System.Net.Http;
using System.Text;

namespace PatientHealthRecords.ApiService;

public interface IPhrApiService
{
	/* Document and Document references operations */
	Task<ApiResult> ProvideDocumentBundleAsync(string accessToken, string jwtProofPrivatekey, string content); 
	Task<ApiResult> CreateDocumentReferenceAsync(string documentReference);
	Task<Bundle?> GetDocumentReferencesForPatientAsync(DocumentSearchParams docFilter);
	//Task<(byte[], string)> FetchDocumentFromRepositoryAsync(string patientId, string uniqueId, string homeCommunityId, string repositoryUniqueId);
	
	/* ValueSet Operations */
	Task<ValueSet?> GetValuesetAsync(string valueSetOid);

	/* Patient Operations */
	Task<Hl7.Fhir.Model.Patient> GetPixReferenceForPerson(string personId);

	/* PersonService operations*/
	//Task<PersontjenestenPerson?> GetPersonByPostAsync(string postPersonIdentifier);

}

/// <summary>
/// Service class for Patient Health Records API operations
/// </summary>
/// <param name="httpClient">client to make the server calls</param>
public class PhrApiService(HttpClient _httpClient, IPhrApiCaller _apiCaller, PhrApiOptions _apiOptions) : IPhrApiService
{
	/// <summary>
	/// Post new document, including document reference's metainformation to repository in Norsk helsenett via BFF
	/// </summary>
	/// <param name="content"></param>
	/// <returns>status of creation</returns>
	public async Task<ApiResult> ProvideDocumentBundleAsync(string accessToken, string jwtProofPrivatekey, string content)
	{
		var headerKeyValues = new List<KeyValuePair<string, string>>();

		/*if (sessionLoginResult.DocumentListScopeId == (int)DocumentListScopeEnum.Kjernejournalforskriften)
		{
			// Only send access basis if using scope 'nhn:phr/mhd/read-documentreferences/kjernejournalforskriften'
			headerKeyValues.Add(new KeyValuePair<string, string>("nhn-access-basis", sessionLoginResult.GrunnlagId ?? string.Empty));
		}*/

		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-source-system", "PJD Test EPJ"));
		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()));

		//var apiUrl = _apiUrlSettings.GetDocumentReferencesApiUrl;
		var apiUrl = _apiOptions.ProvideDocumentBundleUrl;
		
		StringContent stringContent = new StringContent(content, Encoding.UTF8, mediaType: "application/fhir+json"); 

		var apiResult = await _apiCaller.CallApi(
			HttpMethod.Post,
			apiUrl,
			accessToken,
			jwtProofPrivatekey: jwtProofPrivatekey,
			document: false,
			headerKeyValues: headerKeyValues,
			acceptHeaderValues: ["application/fhir+json; fhirVersion=4.0"], // Only empty accept header, or 'application/fhir+json' is allowed				
			content: stringContent
			//formUrlEncodedContent: urlencodedContent
			);
	
		return apiResult;		
	}

	/// <summary>
	/// Post new document, including document reference's metainformation to repository in Norsk helsenett via BFF
	/// </summary>
	/// <param name="content"></param>
	/// <returns>status of creation</returns>
	public async Task<ApiResult> CreateDocumentReferenceAsync(string content)
	{
		var headerKeyValues = new List<KeyValuePair<string, string>>();

		/*if (sessionLoginResult.DocumentListScopeId == (int)DocumentListScopeEnum.Kjernejournalforskriften)
		{
			// Only send access basis if using scope 'nhn:phr/mhd/read-documentreferences/kjernejournalforskriften'
			headerKeyValues.Add(new KeyValuePair<string, string>("nhn-access-basis", sessionLoginResult.GrunnlagId ?? string.Empty));
		}*/

		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-source-system", "PJD Test EPJ"));
		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()));

		//var apiUrl = _apiUrlSettings.GetDocumentReferencesApiUrl;
		var apiUrl = _apiOptions.GetDocumentReferencesApiUrl;

		/*var urlencodedContent = new FormUrlEncodedContent([
			new("patient.identifier", sessionLoginResult.PatientId),
			new("status", "current"),
			new("_count", pageSize.ToString()),
			new("_page", pageIndex.ToString()),
			new("_sort", sort)
		]);

		var apiResult = await _apiCaller.CallApi("POST",
			apiUrl,
			sessionLoginResult.AccessToken!,
			jwtProofPrivatekey: null,
			document: false,
			headerKeyValues: headerKeyValues,
			acceptHeaderValues: ["application/fhir+json; fhirVersion=4.0"], // Only empty accept header, or 'application/fhir+json' is allowed				
			formUrlEncodedContent: urlencodedContent
			);

		*/

		return null; 	
	}

	public Task<Bundle?> GetDocumentReferencesForPatientAsync(DocumentSearchParams docFilter)
	{
		throw new NotImplementedException();
	}

	public Task<Patient> GetPixReferenceForPerson(string personId)
	{
		throw new NotImplementedException();
	}

	public Task<ValueSet?> GetValuesetAsync(string valueSetOid)
	{
		throw new NotImplementedException();
	}
}
