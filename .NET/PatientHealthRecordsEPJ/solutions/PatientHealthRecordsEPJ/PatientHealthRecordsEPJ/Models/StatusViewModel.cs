using PatientHealthRecordsEPJ.Personverninnstillinger;

namespace PatientHealthRecordsEPJ.Models;

public class StatusViewModel
{
	public bool IsSuccess { get; set; }
	public bool IsSuccessRawPvk { get; set; }
	public string ErrorMessage { get; set; }

	public StatusResult? Status { get; set; }
	public StatusPvkResult? StatusPvk { get; set; } // Raw PVK settings

	public bool IsHprNumberBlocked { get; set; }
	public List<Navngitthelseperson>? NavngittHelsepersoner { get; set; } = null!;

	public string? JsonStatus { get; set; }

	public ApiResult ApiResult { get; set; }	
}

