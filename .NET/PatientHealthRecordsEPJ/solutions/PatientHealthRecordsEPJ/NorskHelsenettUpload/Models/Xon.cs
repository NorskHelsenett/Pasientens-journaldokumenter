using Fhir.Metrics;

namespace NorskHelsenettUpload.Models;

public class Xon
{
    public string Name { get;set; } = string.Empty; 
    public string NameTypeCode { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string CheckDigit { get; set; } = string.Empty;
    public string CheckDigitScheme { get; set; } = string.Empty;
    public string AssigningAuthority { get; set; } = string.Empty;
    public string IdentifierTypeCode { get; set; } = string.Empty;
    public string AssigningFacility { get; set; } = string.Empty;
    public string NameRepresentationCode { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;


    public string Hl7String { get; set; } = string.Empty;

    public Xon Parse(string input)
    {
        var splitInput = input.Split('^');

        if (splitInput.Length == 7)
        {
            Name = splitInput[0];
            NameTypeCode = splitInput[1];
            IdNumber = splitInput[2];
            CheckDigit = splitInput[3];
            CheckDigitScheme = splitInput[4];
            AssigningAuthority = splitInput[5];
            IdentifierTypeCode = splitInput[6];
            AssigningFacility = splitInput[7];
            NameRepresentationCode = splitInput[8];
            Identifier = splitInput[9];

            return this;
        }
        else
        {
            throw new ArgumentOutOfRangeException("Invalid string length to parse");
        }

    }

    public string ToHl7String()
    {
        return $"{Name}^{NameTypeCode}^{IdNumber}^{CheckDigit}^{CheckDigitScheme}^{AssigningAuthority}^{IdentifierTypeCode}^{AssigningFacility}^{NameRepresentationCode}^{Identifier}";
    }
}


