using Hl7.Fhir.Model;

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

public class TypeAndValue
{
	public string Type { get; set; }
	public string? Value { get; set; }
}

public class DocumentInfo
{
	public string Title { get; set; } = string.Empty;
	public string? DocumentReferenceId { get; set; }
	public string? DocumentUniqueId { get; set; }
	public List<TypeAndValue> Details { get; set; } = new();
}