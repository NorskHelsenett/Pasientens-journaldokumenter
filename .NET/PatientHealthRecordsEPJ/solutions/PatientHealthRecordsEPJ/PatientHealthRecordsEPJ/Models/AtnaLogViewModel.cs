using Hl7.Fhir.Model;
using PatientHealthRecordsEPJ.Personverninnstillinger;

namespace PatientHealthRecordsEPJ.Models;

public class AtnaLogViewModel
{
	public bool IsSuccess { get; set; }
	public bool IsSuccessRawPvk { get; set; }
	public string ErrorMessage { get; set; }

	public Bundle? Bundle { get; set; }

	public List<Bundle.EntryComponent> Entries { get; set; } = new List<Bundle.EntryComponent>();

    public string? SelectedHomeCommunityId { get; set; }

	public Dictionary<string, string> GuidToPatientIdentifierMap { get; set; } = new Dictionary<string, string>();	

	public int PageSize { get; set; }
	public string SortExpression { get; set; }

	public Pagination Pagination { get; set; }

	//public StatusResult? Status { get; set; }
	//public StatusPvkResult? StatusPvk { get; set; } // Raw PVK settings

	public string? JsonAuditEvents { get; set; }

	public ApiResult ApiResult { get; set; }	
}

