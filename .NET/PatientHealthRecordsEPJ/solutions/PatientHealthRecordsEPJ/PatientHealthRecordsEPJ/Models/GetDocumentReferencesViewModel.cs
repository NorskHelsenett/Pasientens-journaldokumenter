using Hl7.Fhir.Model;
using System.Text.Json.Serialization;

namespace PatientHealthRecordsEPJ.Models; 

public class GetDocumentReferencesViewModel
    {
        public bool IsSuccess  { get; set; }
	public string ErrorMessage { get; set; }

	public Bundle Bundle { get; set; }

	public int PageSize { get; set; }
	public string SortExpression { get; set; }

	public bool UseCaching { get; set; }

	public int BlockedCount { get; set; }

	public Pagination Pagination { get; set; }

	public ApiResult ApiResult { get; set; }

    public int? NorsCount { get; set; } // Antall sperrede dokumenter
	public bool IsRetrievedFromCache { get; set; } // If true, the response was retrieved from cache. This is determined by the presence of the "NHN-Retrieved-From-Cache" header with value "true" in the response.

	public List<UnavailableLocation> UnavailableLocations { get; set; } = new List<UnavailableLocation>(); 
}

public class GetDocumentViewModel
{
	public bool IsSuccess { get; set; }
	public string ErrorMessage { get; set; }

	public bool IsShowClinicalDocumentXml { get; set; }
	public string ClinicalDocumentXml { get; set; }

	public Bundle Bundle { get; set; }		

	public int PageSize { get; set; }
	public string SortExpression { get; set; }

	public int BlockedCount { get; set; }

	public ApiResult ApiResult { get; set; }

	public Pagination Pagination { get; set; }

	public string? ContentType { get; set; }
	public string? Html { get; set; }	
	public string? ImageBase64String { get; set; }
	public string? DocumentUrl { get; set; }
	public string? ClinicalDocumentUrl { get; set; }
	public bool IsConverted { get; set; }
    public string? ConvertedFromType { get; set; }
	public string? ConvertedToType { get; set; }
}

public class UnavailableLocation
{
	[JsonPropertyName("location")]
	public string Location { get; set; }
	[JsonPropertyName("locationName")]
	public string LocationName { get; set; }
	[JsonPropertyName("errorCode")]
	public string ErrorCode { get; set; }

}
