using PatientHealthRecordsEPJ.Personverninnstillinger;

namespace PatientHealthRecordsEPJ.Models;

public class HelseIdViewModel
{
	public bool IsSuccess { get; set; }
	public bool IsSuccessRawPvk { get; set; }
	public string ErrorMessage { get; set; }

	//public string? AccessToken { get; set; }

	public string? PhrAccessToken { get; set; }
	public string? RepositoryAccessToken { get; set; }
	public string? ServiceAccessToken { get; set; }

	public string? PhrJwtToken { get; set; }
	public string? RepositoryJwtToken { get; set; }
	public string? ServiceJwtToken { get; set; }
}

