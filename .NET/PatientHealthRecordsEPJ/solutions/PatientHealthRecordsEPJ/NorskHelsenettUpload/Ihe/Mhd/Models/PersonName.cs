using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NorskHelsenettUpload.Ihe.Mhd.Models;

public class PersonName
{
    [Required(AllowEmptyStrings = false)]
    public string? FirstName { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string FamilyName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }

    public override string ToString()
    {
        var s = new StringBuilder(); 

        s.Append(FirstName);
        
        if (MiddleName != null)
            s.Append(' ' + MiddleName);

		if (FamilyName != null)
			s.Append(' ' + FamilyName);

        return s.ToString(); 
    }

}
