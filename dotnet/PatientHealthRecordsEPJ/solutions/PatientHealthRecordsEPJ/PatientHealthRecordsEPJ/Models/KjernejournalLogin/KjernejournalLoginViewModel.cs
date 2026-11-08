namespace PatientHealthRecordsEPJ.Models;

public class KjernejournalLoginViewModel
{
	public string PatientNIN { get; set; }

	public bool IsSuccess { get; set; }
	public string ErrorMessage { get; set; }
	
	public KjernejournalSessionCreated? KjernejournalSession { get; set; } = null;

}
