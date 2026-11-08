namespace NorskHelsenettUpload.Ihe.Mhd.Models;

public class XdsRequest
{
    public Document DocumentWithEntry { get; set; } = new Document();
    public DocumentSubmissionMetadata Submission { get; set; } = new DocumentSubmissionMetadata();
    public Association Association { get; set; } = new Association();
}
