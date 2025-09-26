using System.ComponentModel.DataAnnotations;

namespace NorskHelsenettUpload.Ihe.Mhd.Models;

public class DocumentSubmissionMetadata
{

    public const string AvailabilityStatusApproved = "urn:oasis:names:tc:ebxml-regrep:StatusType:Approved";
	public const string AvailabilityStatusDeprecatd = "urn:oasis:names:tc:ebxml-regrep:StatusType:Deprecated";

	[Required]
    public Guid UniqueId = Guid.NewGuid();
    [Required]
    public DateTime SubmissionTime { get; set; }
    [Required]
    public string AvailabilityStatus { get; set; }

    public string SourceId { get; set; }
    [Required]
    public string PatientId { get; set; }

    public string? Title { get; set; }

    [StringLength(255)]
    public string? Comments { get; set; }
    
    [Required]
    //public Author[]? Author { get; set; }  = [];
	public List<Author> Author { get; set; } = [];
}
