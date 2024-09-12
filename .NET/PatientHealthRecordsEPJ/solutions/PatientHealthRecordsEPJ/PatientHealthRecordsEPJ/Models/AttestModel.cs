using Microsoft.Extensions.Primitives;

namespace PatientHealthRecordsEPJ.Models; 

public class AttestModel
{
    //public string Identifier { get; set; }
	public string Authorization { get; set; } = default!;
	public string LegalIdentity { get; set; } = default!;
	public string PointOfCare { get; set; } = default!;
	public string Department { get; set; } = default!;
	public string HealthcareService { get; set; } = default!;
	public string PurposeOfUse { get; set; } = default!;
	public string PurposeOfUseDetails { get; set; } = default!;
	public string PatientPointOfCare { get; set; } = default!;
	public string PatientDepartment { get; set; } = default!;

	public static AttestModel Build(
		string organizationId, 
		string healthPersonellUserId,
		string authorizationId
		)
	{
		var attest = new AttestModel();

		var legalEntityAndPointOfCare = HomeViewModel.OrganizationMappingList.FirstOrDefault(x => x.Id == organizationId)!; 

		//attest.Identifier = string.Empty; 
		attest.Authorization = authorizationId;
		attest.LegalIdentity = legalEntityAndPointOfCare.LegalEntity;
		attest.PointOfCare = legalEntityAndPointOfCare.PointOfCare;
		attest.Department = "12345";
		attest.HealthcareService = "01";
		attest.PurposeOfUse = "TREAT";
		attest.PurposeOfUseDetails = "BEHANDLER";
		attest.PatientPointOfCare = legalEntityAndPointOfCare.PointOfCare; 
		attest.PatientDepartment = "12345";	

		return attest; 
	}
}	
