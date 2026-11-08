namespace PatientHealthRecordsEPJ.Models.DokumentlagerTest;


// POCOs for deserialisering request to /registry-repository/document-status endpoint in PHR REST API 
// This endpoint forwards the request to the instance of XcaDService running in the test environment, which returns the status of a document in the document repository/registry.
public class DocumentStatusResult
{
	public DocumentStatusDocument document { get; set; }
	public bool success { get; set; }
	public object errors { get; set; }
	public string message { get; set; }
}

public class DocumentStatusDocument
{
	public string documentId { get; set; }
	public string homeCommunityId { get; set; }
	public string repositoryUniqueId { get; set; }
	public bool isFullySaved { get; set; }
	public bool savedToRegistry { get; set; }
	public bool savedToRepository { get; set; }
}
