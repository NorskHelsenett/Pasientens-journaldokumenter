using NorskHelsenettUpload.Models;
using System.ComponentModel.DataAnnotations;

namespace NorskHelsenettUpload.Ihe.Mhd.Models;

public class Author
{
    [Required]
    public Xcn? Name { get; set; } = new Xcn();
    public Xon[]? InstitutionName { get; set; } = [new Xon()];
    public string[]? Role { get; set; } = [];
    public string? Specialty { get; set; }
}
