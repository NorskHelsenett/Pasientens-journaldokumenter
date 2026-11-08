namespace PatientHealthRecordsEPJ.Models;

public class KjernejournalCreateSessionModel
{
	public string? ehr_code_challenge { get; set; }
	public Claims? claims { get; set; }
}

public class Claims
{
	public Patient_Identifier? patient_identifier { get; set; }
	public Access_Basis? access_basis { get; set; }
	public Practitioner_Authorization? practitioner_authorization { get; set; }
}

public class Patient_Identifier
{
	public string? id { get; set; }
	public string? system { get; set; }
	public string? authority { get; set; }
}

public class Access_Basis
{
	public string? code { get; set; }
	public string? system { get; set; }
	public string? assigner { get; set; }
}

public class Practitioner_Authorization
{
	public string? code { get; set; }
	public string? system { get; set; }
	public string? assigner { get; set; }
}

public class KjernejournalSessionCreated
{
	public string? code { get; set; }
	public string? sessionId { get; set; }
}
