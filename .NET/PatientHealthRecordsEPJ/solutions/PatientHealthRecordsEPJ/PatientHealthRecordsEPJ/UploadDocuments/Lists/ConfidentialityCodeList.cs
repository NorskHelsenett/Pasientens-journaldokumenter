using NorskHelsenettUpload;

namespace PatientHealthRecordsEPJ.UploadDocuments.Lists; 
	
public class ConfidentialityCodeList
{
	public static List<SimpleCoding> ConfidentialityCodes = new List<SimpleCoding>()
	{
		new SimpleCoding() { System = "2.16.840.1.113883.5.25", Code = "N", Display = "Normal" },
		new SimpleCoding() { System = "2.16.840.1.113883.5.25", Code = "R", Display = "Restricted" },
		new SimpleCoding() { System = "2.16.840.1.113883.5.25", Code = "V", Display = "Very restricted" }, 
        new SimpleCoding() { System = "2.16.840.1.113883.5.25", Code = "L", Display = "Low" },
		new SimpleCoding() { System = "2.16.840.1.113883.5.25", Code = "U", Display = "Unrestricted" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORT", Display = "Not allowed/accessible for healthcare personell" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_FFL", Display = "Not visible for patient - security reasons" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_FORANS", Display = "Not visible for people with parenting responsibility" }
	}; 
}
