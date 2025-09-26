using NorskHelsenettUpload;

namespace PatientHealthRecordsEPJ.UploadDocuments.Lists; 
	
public class PracticeSettingCodeList
{
	public static List<SimpleCoding> PracticeSettingCodes = new List<SimpleCoding>()
	{
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.8653", Code = "1", Display = "Pleietjenester" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.8653", Code = "4", Display = "Legevakt" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.8653", Code = "6", Display = "Akuttmottak" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.8653", Code = "7", Display = "Ambulansetjeneste, ordinær" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.8653", Code = "8", Display = "Luftambulanse" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.8653", Code = "9", Display = "AMK-sentral" }, // ⚠️ had missing Display in HTML
        new SimpleCoding() { System = "2.16.578.1.12.4.1.1.8651", Code = "A02", Display = "Observasjonsenhet" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.8651", Code = "A03", Display = "Intensivenhet" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.8651", Code = "A04", Display = "Overvåkningsenhet" }
	}; 
}
