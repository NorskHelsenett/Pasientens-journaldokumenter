using Hl7.Fhir.Model;
using NorskHelsenettUpload.Models;
using System.Net.Http;
using System.Text.Json;

namespace PatientHealthRecordsEPJ.Services;

public class OrganizationService 
{
	private readonly IHttpClientFactory _httpClientFactory;

	public OrganizationService(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory;
	}

	//[HttpGet("{identifier}")]
	public async Task<OrgRegOrganization> GetOrganizationByOrgNrAsync(string identifier)
	{
		var httpClientOrganizationApi = _httpClientFactory.CreateClient("OrganizationAPI");

		var apiUrl = httpClientOrganizationApi.BaseAddress + $"organization/{identifier}";

		var response = await httpClientOrganizationApi.GetAsync(apiUrl);

		response.EnsureSuccessStatusCode();

		if (response != null)
		{
			var content = response.Content.ReadAsStringAsync();
			var org = JsonSerializer.Deserialize<OrgRegOrganization>(content.Result);

			return org; 
			
			//return Ok(await response.Content.ReadAsStreamAsync());
		}
		else
		{
			//return BadRequest($"Something went wrong when trying to retrive org.details for org.nr.#{identifier}");
			return null; 
		}

	}

	public async Task<Hl7.Fhir.Model.Organization> GetHl7OrganizationDetailsAsync(string orgIdentifier)
	{	
		var org = await GetOrganizationByOrgNrAsync(orgIdentifier);

		var organization = GetHl7OrganizationDetailFromOrganization(org); 
		
		return organization;
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
