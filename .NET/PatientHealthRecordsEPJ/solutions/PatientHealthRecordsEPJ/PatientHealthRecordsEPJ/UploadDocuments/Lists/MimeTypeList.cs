namespace PatientHealthRecordsEPJ.UploadDocuments.Lists; 
	
public class MimeTypeList
{
	public static List<TextValue> MimeTypes = new List<TextValue>()
	{
		  new TextValue() { Value = "image/png", Text = "PNG-image" },
		  new TextValue() { Value = "image/jpeg", Text = "JPEG-image" },
		  new TextValue() { Value = "image/gif", Text = "GIF-image" },
		  new TextValue() { Value = "image/tiff", Text = "TIFF-image" },
		  new TextValue() { Value = "application/pdf", Text = "PDF-file" },
		  new TextValue() { Value = "text/plain", Text = "TEXT-file (plain)" }, // Supported 
		  new TextValue() { Value = "application/rtf", Text = "RTF-document" },
		  new TextValue() { Value = "application/xml", Text = "XML-document (Machine readable)" },
		  new TextValue() { Value = "text/xml", Text = "XML-document (Human readable)" },
		  new TextValue() { Value = "application/fhir+xml", Text = "FHIR-document (XML)" },
		  new TextValue() { Value = "application/fhir+json", Text = "FHIR-document (JSON)" }		 

	}; 
}
