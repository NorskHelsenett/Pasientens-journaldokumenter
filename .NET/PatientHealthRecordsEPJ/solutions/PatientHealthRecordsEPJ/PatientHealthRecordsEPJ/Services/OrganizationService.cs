using Hl7.Fhir.Model;
using NorskHelsenettUpload.Models;
using System.Net.Http;
using System.Text.Json;

namespace PatientHealthRecordsEPJ.Services;

public class OrganizationService 
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger<OrganizationService> _logger;

	public OrganizationService(IHttpClientFactory httpClientFactory, ILogger<OrganizationService> logger)
	{
		_httpClientFactory = httpClientFactory;
		_logger = logger;
	}
	
	public async Task<OrgRegOrganization?> GetOrganizationByOrgNrAsync(string identifier)
	{
		var httpClientOrganizationApi = _httpClientFactory.CreateClient("OrganizationAPI");

		var apiUrl = httpClientOrganizationApi.BaseAddress + $"organization/{identifier}";
		//var apiUrl = httpClientOrganizationApi.BaseAddress + $"brreg/{identifier}";
			
		try
		{
			var response = await httpClientOrganizationApi.GetAsync(apiUrl);

			var content = response.Content.ReadAsStringAsync();
			var org = JsonSerializer.Deserialize<OrgRegOrganization>(content.Result);

			return org;
		}
		catch (Exception ex)
		{
			_logger.LogError($"Could not read organization from {apiUrl}"); 
		}		
		
		return null; 
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
