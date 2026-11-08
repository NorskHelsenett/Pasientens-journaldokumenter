using NorskHelsenettUpload;

namespace PatientHealthRecordsEPJ.UploadDocuments.Lists; 

public class HealthcareFacilityTypeList
{
	public static List<SimpleCoding> HealthcareFacilityTypes = new List<SimpleCoding>()
	{
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.1303", Code = "86.101", Display = "Alminnelige somatiske sykehus" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.1303", Code = "86.102", Display = "Somatiske spesialsykehus" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.1303", Code = "86.103", Display = "Andre somatiske spesialinstitusjoner" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.1303", Code = "86.211", Display = "Allmenn legetjeneste" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.1303", Code = "86.212", Display = "Somatiske poliklinikker" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.1303", Code = "86.901", Display = "Hjemmesykepleie" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.1303", Code = "86.902", Display = "Fysioterapitjeneste" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.1303", Code = "86.903", Display = "Helsestasjons- og skolehelsetjeneste" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.1303", Code = "86.907", Display = "Ambulansetjenester" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.1303", Code = "86.909", Display = "Andre helsetjenester" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.1303", Code = "87.102", Display = "Somatiske sykehjem" },
		new SimpleCoding() { System = "http://snomed.info/sct", Code = "257622000", Display = "General practice clinic (environment)" }
		
	}; 
}
