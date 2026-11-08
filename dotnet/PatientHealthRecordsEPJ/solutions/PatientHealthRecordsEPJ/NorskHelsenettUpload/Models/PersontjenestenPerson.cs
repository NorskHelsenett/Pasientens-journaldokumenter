namespace NorskHelsenettUpload.Models;

public class PersontjenestenPerson
{
    public string id { get; set; }
    public int sequenceNumber { get; set; }
    public object falseIdentity { get; set; }
    public Norwegianidentificationnumber[] norwegianIdentificationNumber { get; set; }
    public object[] identityVerification { get; set; }
    public object[] residuaryEstateContactInformation { get; set; }
    public object[] identificationDocument { get; set; }
    public object[] status { get; set; }
    public object[] immigrationAuthoritiesIdentificationNumber { get; set; }
    public object[] foreignPersonIdentificationNumber { get; set; }
    public object[] sharedResidence { get; set; }
    public Gender[] gender { get; set; }
    public Birth[] birth { get; set; }
    public object[] birthInNorway { get; set; }
    public object[] familyRelation { get; set; }
    public object[] maritalStatus { get; set; }
    public object death { get; set; }
    public Name[] name { get; set; }
    public object[] addressProtection { get; set; }
    public object[] residentialAddress { get; set; }
    public object[] presentAddress { get; set; }
    public object[] immigrationToNorway { get; set; }
    public object[] emigrationFromNorway { get; set; }
    public object[] useOfSamiLanguage { get; set; }
    public object[] samiParliamentElectoralRegistryStatus { get; set; }
    public object[] preferredContactAddress { get; set; }
    public object[] postalAddress { get; set; }
    public object[] foreignPostalAddress { get; set; }
    public object[] parentalResponsibility { get; set; }
    public object[] citizenship { get; set; }
    public object[] citizenshipRetention { get; set; }
    public object[] residencePermit { get; set; }
    public object[] stayOnSvalbard { get; set; }
    public object[] guardianshipOrFuturePowerOfAttorney { get; set; }
    public object[] deprivedLegalAuthority { get; set; }
    public object[] legalAuthority { get; set; }
    public object commonContactRegisterInformation { get; set; }
}

public class Norwegianidentificationnumber
{
    public string status { get; set; }
    public string identificationNumber { get; set; }
    public string identificationNumberType { get; set; }
    public DateTime registeredAt { get; set; }
    public bool isValid { get; set; }
    public string source { get; set; }
    public object reason { get; set; }
    public object validFrom { get; set; }
    public object validTo { get; set; }
}

public class Gender
{
    public string gender { get; set; }
    public object registeredAt { get; set; }
    public bool isValid { get; set; }
    public string source { get; set; }
    public object reason { get; set; }
    public object validFrom { get; set; }
    public object validTo { get; set; }
}

public class Birth
{
    public DateTime birthDate { get; set; }
    public string birthYear { get; set; }
    public object birthPlace { get; set; }
    public string birthMunicipalityNumber { get; set; }
    public string birthMunicipalityName { get; set; }
    public string birthCountyNumber { get; set; }
    public string birthCountyName { get; set; }
    public string birthCountry { get; set; }
    public DateTime registeredAt { get; set; }
    public bool isValid { get; set; }
    public string source { get; set; }
    public object reason { get; set; }
    public DateTime validFrom { get; set; }
    public object validTo { get; set; }
}

public class Name
{
    public string givenName { get; set; }
    public object middleName { get; set; }
    public string familyName { get; set; }
    public object shortName { get; set; }
    public object originalName { get; set; }
    public DateTime registeredAt { get; set; }
    public bool isValid { get; set; }
    public string source { get; set; }
    public string reason { get; set; }
    public DateTime validFrom { get; set; }
    public object validTo { get; set; }
}
