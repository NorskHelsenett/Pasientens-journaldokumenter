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

	public int BlockedCount { get; set; }

	public Pagination Pagination { get; set; }

        public ApiResult ApiResult { get; set; }

        public int? NorsCount { get; set; } // Antall sperrede dokumenter

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

	public Pagination Pagination { get; set; }

	public string? ContentType { get; set; }
	public string? Html { get; set; }
	public string? PdfBase64String { get; set; }
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
