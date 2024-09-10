using IdentityModel.Client;
using IdentityModel.OidcClient.DPoP;

public class ApiUrlSettings
{
	public string GetDocumentReferencesApiUrl { get; set; } = default!; 
	public string GetDocumentApiUrl { get; set; } = default!;
	public string GetSamlAssertionUrl { get; set; } = default!;
}