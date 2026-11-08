using Hl7.Fhir.Model;

namespace PatientHealthRecords.ApiService.Ihe.Mhd.Models;

public class DocumentSearchParams
{
    public string PatientId { get; set; }
    string? AuthorGiven { get; set; }
    string? AuthorFamily { get; set; }
    string? Category { get; set; }
    string[]? CreationTimes { get; set; }
    DateTime? @Date { get; set; }
    string? EventCode { get; set; }
    string? Facility { get; set; }
    string? FormatCode { get; set; }
    string? Identifier { get; set; }
    string[]? ServicePeriods { get; set; }
    string? RelatedReferenceIds { get; set; }
    public string? SecurityLabel { get; set; }
    string? PracticeSettingCode { get; set; }
    public DocumentReferenceStatus AvailabilityStatus { get; set; }
    string? TypeCode { get; set; }
}
