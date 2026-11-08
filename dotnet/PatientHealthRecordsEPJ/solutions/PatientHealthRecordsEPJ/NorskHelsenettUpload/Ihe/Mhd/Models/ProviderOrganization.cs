using System.ComponentModel.DataAnnotations;

namespace NorskHelsenettUpload.Ihe.Mhd.Models;

/// <summary>
/// Represents the provider organization from CDA recordTarget/patientRole/providerOrganization.
/// </summary>
public class ProviderOrganization
{
	/// <summary>
	/// Organization name (e.g., "Mikrobiologisk avdeling").
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Organization identifier (CDA: @extension).
	/// </summary>
	public string? Identifier { get; set; }

	/// <summary>
	/// Assigning authority (CDA: @root).
	/// </summary>
	public string? AssigningAuthority { get; set; }

	/// <summary>
	/// Optional parent organization (CDA: asOrganizationPartOf/wholeOrganization).
	/// </summary>
	public ParentOrganization? ParentOrganization { get; set; }
}

/// <summary>
/// Represents a parent (wholeOrganization) element in CDA.
/// </summary>
public class ParentOrganization
{
	/// <summary>
	/// Parent organization name (e.g., "Sykehuset Østfold HF").
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Organization identifier (CDA: @extension).
	/// </summary>
	public string? Identifier { get; set; }

	/// <summary>
	/// Assigning authority (CDA: @root).
	/// </summary>
	public string? AssigningAuthority { get; set; }
}
