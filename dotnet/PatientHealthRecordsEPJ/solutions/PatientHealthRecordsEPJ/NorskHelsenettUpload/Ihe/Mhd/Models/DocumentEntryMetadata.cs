using Hl7.Fhir.Model;
using NorskHelsenettUpload.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Xml;

namespace NorskHelsenettUpload.Ihe.Mhd.Models;

public class DocumentEntryMetadata
{
    [Required]
    public Guid UniqueId { get; set; }
    [Required(ErrorMessage = "Document MUST be assigned to the patient")]

	/// <summary>
	/// PatientId:  The official patient identifier for the document in the XDS Registry.
	/// Must be a globally unique identifier within the affinity domain(typically CX format: assigning authority + ID).
	/// This is the primary patient identifier used for indexing, querying, and matching documents.
	/// Example: PatientId = "123456^^^&1.2.840.114350.1.13.0.1.7&ISO"
	/// </summary>
	public string? PatientId { get; set; }

	/// <summary>
	/// SourcePatientId:  The patient identifier as known by the source system that submitted the document.
	/// It may not be globally unique — it reflects the local patient ID in the originating EHR or departmental system.
	/// Stored in the registry so that if there’s ever reconciliation or troubleshooting, you can trace back to the local source system’s patient identity.
	/// Example: SourcePatientId = "987654^^^&2.16.840.1.113883.19.3&ISO"
	/// </summary>
	/// 
	public string? SourcePatientId { get; set; }

    public Xcn? LegalAuthenticator { get; set; } // = new Xcn();
    public DateTime ServiceStartTime { get; set; }
    public DateTime ServiceStopTime { get; set; }
    public SourcePatientInfo? SourcePatientInfo { get; set; } = new();
	public ProviderOrganization? ProviderOrganization { get; set; }
	[Required]
    public string? ClassCode { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Document MUST have at least 1 confidentiality code")]
	//public string[] ConfidentialityCodeList { get; set; } = [];
	//public List<string> ConfidentialityCodeList_Hl7 { get; set; } = []; // N, R, V, etc. (HL7 confidentiality codes)
	public string ConfidentialityCode_Hl7 { get; set; } = string.Empty; // N, R, V, etc. (HL7 confidentiality codes)
	public List<string> ConfidentialityCodeList_9603 { get; set; } = [];

	public List<string> AllConfidentialityCodes
	{
		get
		{
			return new List<string> { ConfidentialityCode_Hl7 }.Concat(ConfidentialityCodeList_9603).ToList();
		}
	}

	[Required]
    public string? FormatCode { get; set; }
    [Required]
    public string? HealthCareFacilityType { get; set; }
    [Required]
    public string? PracticeSettingCode { get; set; }
    [Required]
    public string? TypeCode { get; set; }
    //public string[]? EventCodeList { get; set; } = [];
	public List<string>? EventCodeList { get; set; } = [];
	[Required(AllowEmptyStrings = false, ErrorMessage = "Document MUST have at least 1 author")]
    //public Author[] Authors { get; set; } = [];
	public List<Author> Authors { get; set; } = [];
	public string? Comment { get; set; }
	//public string? HomeCommunityId { get; set; }
	//public string? RepositoryUniqueId { get; set; }

	// Special handling for deparment	
	public string? DepartmentAssigningAuthoritySystem { get; set; }
	public string? DepartmentIdentifier { get; set; }    // For RESH-ID, which is the unique identifier for a department in the Norwegian Specialist Health Service (Spesialisthelsetjenesten). It is used to identify the specific department or unit within a healthcare organization that is responsible for the document or service. The RESH-ID is part of the Norwegian healthcare system's organizational structure and is used for administrative and reporting purposes.
	public string? DepartmentName { get; set; }
}
