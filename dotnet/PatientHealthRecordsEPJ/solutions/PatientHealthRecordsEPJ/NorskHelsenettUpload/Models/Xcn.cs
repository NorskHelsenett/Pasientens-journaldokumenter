using NorskHelsenettUpload.Ihe.Mhd.Models;

namespace NorskHelsenettUpload.Models;

public class Xcn
{
    public string PersonIdentifier { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    // Second and Further Given Names or Initials Thereof
    public string MiddleName { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;

    public string Hl7String { get; set;} = string.Empty;

    public Xcn Parse(string input)
    {
        var splitInput = input.Split('^');

        if (splitInput.Length == 7)
        {
            PersonIdentifier = splitInput[0];
            FamilyName = splitInput[1];
            GivenName = splitInput[2];
            MiddleName = splitInput[3];
            Suffix = splitInput[4];
            Prefix = splitInput[5];
            Degree = splitInput[6];

            return this;
        }
        else
        {
            throw new ArgumentOutOfRangeException("Invalid string length to parse");
        }
        
    }

    public string ToHl7String()
    {
        return $"{PersonIdentifier}^{FamilyName}^{GivenName}^{MiddleName}^{Suffix}^{Prefix}^{Degree}";
    }

	public string ToFullName()
	{
		var name = GivenName;

		if (!string.IsNullOrWhiteSpace(MiddleName))
		{
			name += " " + MiddleName;
		}

		name += " " + FamilyName;

        return name;
	}
}


