namespace PatientHealthRecords.ApiService;

public class PhrApiOptions
{
	public string? ActiveEnvironment { get; set; } = default!; // test, qa
	public List<PhrApiEnvironment> Environments { get; set; } = default!;
}

public class PhrApiEnvironment
{
	public string Environment { get; set; } = default!;
	public string RootUrl { get; set; } = default!; 
	public string GetDocumentReferencesApiUrl { get; set; } = default!;
	public string GetDocumentApiUrl { get; set; } = default!;
	public string GetSamlAssertionUrl { get; set; } = default!;
	//public string CreateDocumentReferenceUrl { get; set; } = default!;
	public string GenerateDocumentReferenceMetadataUrl { get; set; } = default!;	
	public string ProvideDocumentBundleUrl { get; set; } = default!;
	public string DeleteDocumentUrl { get; set; } = default!;
	public string PatchDocumentUrl { get; set; } = default!;
	public string StatusUrl { get; set; } = default!;
	public string StatusPvkUrl { get; set; } = default!; // Raw PVK info for debugging purposes
	public string AtnaLogUrl { get; set; } = default!; 
}
