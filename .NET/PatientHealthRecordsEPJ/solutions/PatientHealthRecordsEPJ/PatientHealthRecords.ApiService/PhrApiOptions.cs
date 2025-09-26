namespace PatientHealthRecords.ApiService;

public class PhrApiOptions
{
	public string GetDocumentReferencesApiUrl { get; set; } = default!;
	public string GetDocumentApiUrl { get; set; } = default!;
	public string GetSamlAssertionUrl { get; set; } = default!;
	//public string CreateDocumentReferenceUrl { get; set; } = default!;
	public string GenerateDocumentReferenceMetadataUrl { get; set; } = default!;
	public string ProvideDocumentBundleUrl { get; set; } = default!;
}
