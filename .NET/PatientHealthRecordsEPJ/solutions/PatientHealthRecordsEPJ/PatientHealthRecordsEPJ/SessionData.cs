// This demo project has some very simple session storage to store the authenticated user and associated information to keep things as simple as possible
using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Models;

public class SessionData
{
	public bool IsAuthenticated { get; set; } = false; 

	// Helsepersonell: 
	public string IdentityToken { get; set; } = default!;
	public string AccessToken { get; set; } = default!;
	public string RefreshToken { get; set; } = default!;

	public string PersonId { get; set; } = default!; // Helsepersonell fødselsnummer
	public string Name { get; set; } = default!;
	public string GivenName { get; set; } = default!;
	public string MiddleName { get; set; } = default!;
	public string FamilyName { get; set; } = default!;

	public OrgRegOrganization? OrgRegOrganization { get; set; }	// Organization details received from Persontjenesten

	public string? AuthorizationId { get; set; } = default!;
	public string? AuthorizationName { get; set; } = default!;

	public string? OrganizationId { get; set; } = default!;
	public string? OrganizationName { get; set; } = default!;

	// Pasient: 
	public string? PatientId { get; set; } = default!;
	public string? PatientName { get; set; } = default!;	
	public SourcePatientInfo? SourcePatientInfo { get; set; }

	public string? GrunnlagId { get; set; } = default!; 
	public string? GrunnlagName { get; set; } = default!;

	public string? EnvironmentId { get; set; } = default!;
	public string? EnvironmentName { get; set; } = default!;

	public int DocumentListScopeId { get; set; } = default!;
	public string? DocumentListScopeName { get; set; } = default!;

	// Used to keep state before and after login
	public string? AuthorizeState { get; set; } = default!;

	//
	//	Bundle request
	//
	public NorskHelsenettUpload.ProvideDocumentBundleRequest? BundleRequest { get; set; } //= new NorskHelsenettUpload.ProvideDocumentBundleRequest();	
}