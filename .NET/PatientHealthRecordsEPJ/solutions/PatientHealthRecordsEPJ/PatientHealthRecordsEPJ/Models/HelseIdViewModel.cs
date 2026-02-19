using PatientHealthRecordsEPJ.Personverninnstillinger;

namespace PatientHealthRecordsEPJ.Models;

public class HelseIdViewModel
{
	public bool IsSuccess { get; set; }
	public bool IsSuccessRawPvk { get; set; }
	public string ErrorMessage { get; set; }

	public string? AccessToken { get; set; }
	public string? JwtToken { get; set; }	
}

