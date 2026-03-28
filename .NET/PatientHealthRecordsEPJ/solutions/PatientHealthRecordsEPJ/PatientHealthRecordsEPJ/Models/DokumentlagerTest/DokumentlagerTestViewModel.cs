using PatientHealthRecordsEPJ.Personverninnstillinger;

namespace PatientHealthRecordsEPJ.Models;

public class DokumentlagerTestViewModel
{
	public bool IsSuccess { get; set; }
	public bool IsSuccessRawPvk { get; set; }
	public string ErrorMessage { get; set; }

	public string? TestsJson { get; set; }

	public StatusResult? Status { get; set; }
	public StatusPvkResult? StatusPvk { get; set; } // Raw PVK settings

	public bool IsHprNumberBlocked { get; set; }
	public List<Navngitthelseperson>? NavngittHelsepersoner { get; set; } = null!;

	public string? JsonStatus { get; set; }

	public ApiResult ApiResult { get; set; }

	public DokumentlagerTests Tests { get; set; } = null!; 
}



public class DokumentlagerTestCaseViewModel
{
	public DokumentlagerTestCase Test { get; set; }
	public TestRunInfo Run { get; set; }
	public int GroupIndex { get; set; }
	public int TestIndex { get; set; }
}