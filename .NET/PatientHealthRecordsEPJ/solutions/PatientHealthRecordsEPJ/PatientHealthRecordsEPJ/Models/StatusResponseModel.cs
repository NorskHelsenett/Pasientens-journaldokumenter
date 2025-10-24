namespace PatientHealthRecordsEPJ.Models;

public class StatusResult
{
	public string PatientNIN { get; set; } = default!;
	public PrivacySetting PrivacySetting { get; set; } = default!; 
}

public class PrivacySetting
{
	public bool HasReservationKjernejournal { get; set; }
	public bool IsRestricted { get; set; }
	public bool IsBlocked { get; set; }
}

