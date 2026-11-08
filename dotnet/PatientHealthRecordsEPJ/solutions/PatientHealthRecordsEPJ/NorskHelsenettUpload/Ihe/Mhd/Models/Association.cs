using System.ComponentModel.DataAnnotations;

namespace NorskHelsenettUpload.Ihe.Mhd.Models;

public class Association
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public Guid TargetObject { get; set; }
    [Required]
    public Guid SourceObject { get; set; }
    public string? Status { get; set; }
    [Required(ErrorMessage = "Must select type of association for submission")]
    public string Type { get; set; }
}
