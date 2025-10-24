namespace PatientHealthRecordsEPJ.Models;

public class StatusViewModel
{
	public bool IsSuccess { get; set; }
	public bool IsSuccessRawPvk { get; set; }
	public string ErrorMessage { get; set; }

	public StatusResult? Status { get; set; }
	public StatusPvkResult? StatusPvk { get; set; } // Raw PVK settings

	public string? JsonStatus { get; set; }

	public ApiResult ApiResult { get; set; }	
}

