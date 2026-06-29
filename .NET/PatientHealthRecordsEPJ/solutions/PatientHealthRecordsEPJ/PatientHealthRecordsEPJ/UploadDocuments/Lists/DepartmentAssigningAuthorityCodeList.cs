using NorskHelsenettUpload;

namespace PatientHealthRecordsEPJ.UploadDocuments.Lists; 
	
public class DepartmentAssigningAuthorityCodeList
{
	public static List<SimpleCoding> ClassCodes = new List<SimpleCoding>()
	{
		//new SimpleCoding() { System = "0", Code = "0", Display = "-- Velg type avdeling --" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.4.102", Code = "2.16.578.1.12.4.1.4.102", Display = "RESH-ID (Enheter i Spesialisthelsetjenesten)" },
		new SimpleCoding() { System = "2.16.578.1.12.4.5.390", Code = "2.16.578.1.12.4.5.390", Display = "Avdeling som kun angis med navn (ingen ID)" },
	}; 
}
