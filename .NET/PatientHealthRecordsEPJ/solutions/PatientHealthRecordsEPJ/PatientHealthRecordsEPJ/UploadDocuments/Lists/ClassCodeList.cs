using NorskHelsenettUpload;

namespace PatientHealthRecordsEPJ.UploadDocuments.Lists; 
	
public class ClassCodeList
{
	public static List<SimpleCoding> ClassCodes = new List<SimpleCoding>()
	{
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A00-1", Display = "Epikriser og sammenfatninger" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "B00-1", Display = "Kontinuerlig/løpende journal" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "C00-1", Display = "Prøvesvar, vev og væsker" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D00-1", Display = "Organfunksjon" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "E00-1", Display = "Bildediagnostikk" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "F00-1", Display = "Kurve, observasjon og behandling" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "I00-1", Display = "Korrespondanse" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "S00-1", Display = "Test og scoring" }
	}; 
}
