// This demo project has some very simple session storage to store the authenticated user and associated information to keep things as simple as possible
using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Models;
using PatientHealthRecordsEPJ.UploadDocuments;
using System.Linq;

public class SessionData
{
	public bool IsAuthenticated { get; set; } = false; 

	// Helsepersonell: 
	public string IdentityToken { get; set; } = default!;
	public string AccessToken { get; set; } = default!;
	public string RefreshToken { get; set; } = default!;

	public string PersonId { get; set; } = default!; // Helsepersonell fødselsnummer
	public string? HprNumber { get; set; } = default!;
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

	public bool IsUploadAvailable { get; set; } = default!;	// Set to true if correct upload password (Constants.UploadPassword) has been entered

	//
	//	Bundle request
	//
	public NorskHelsenettUpload.ProvideDocumentBundleRequest? BundleRequest { get; set; } //= new NorskHelsenettUpload.ProvideDocumentBundleRequest();	

	//public int UploadWizardOpenSteps { get; set; }  // How many wizard steps have been opened (how many steps from the start we can navigate directly to)

	//public List<string> UploadWizardOpenSteps { get; set; } = new List<string>(); // Patient, Source, UploadDocument, Classification, Submission, FinalizeSubmission, SubmissionConfirmation, 
	public List<int> UploadWizardOpenSteps { get; set; } = new List<int>(); // Patient, Source, UploadDocument, Classification, Submission, FinalizeSubmission, SubmissionConfirmation, 

	/*public void AddWizardOpenStep(WizardStepsEnum step)
	{
		if (!UploadWizardOpenSteps.Contains(step.ToString()))
		{
			UploadWizardOpenSteps.Add(step.ToString());
		}
	}*/
	public void AddWizardOpenStep(int step)
	{
		if (!UploadWizardOpenSteps.Contains(step))
		{
			UploadWizardOpenSteps.Add(step);
		}
	}
}

