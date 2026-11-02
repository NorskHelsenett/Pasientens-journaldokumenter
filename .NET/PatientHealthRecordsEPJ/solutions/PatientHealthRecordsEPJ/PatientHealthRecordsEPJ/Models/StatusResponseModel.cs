using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PatientHealthRecordsEPJ.Models;

public class StatusResult
{
	public string PatientNIN { get; set; } = default!;
	public PrivacySetting PrivacySetting { get; set; } = default!;
	public DeathDto? Death { get; set; } = default!;
	public AddressProtectionDto? AddressProtection { get; set; } = default!; 
}

public class PrivacySetting
{
	public bool HasReservationKjernejournal { get; set; }
	public bool IsRestricted { get; set; }
	public bool IsBlocked { get; set; }
}

public class DeathDto
{
	public bool IsDeceased { get; set; }
	public DateTime? DeceasedDate { get; set; }
}

public class AddressProtectionDto
{
	public bool HasAddressProtection { get; set; }		// Any protection level
	public bool HasConfidentialAddress { get; set; }    // Confidential or StrictlyConfidential
	public AddressConfidentiality? ProtectionLevel { get; set; }
}

/// <summary>
/// Confidentiality level of an address in the National Population Register.  &lt;br&gt;FREG: Adressegradering
/// </summary>
/// <value>Confidentiality level of an address in the National Population Register.  &lt;br&gt;FREG: Adressegradering</value>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AddressConfidentiality
{
	/// <summary>
	/// Enum Unclassified for value: Unclassified
	/// </summary>
	[EnumMember(Value = "Unclassified")]
	Unclassified = 1,

	/// <summary>
	/// Enum ClientAddress for value: ClientAddress
	/// </summary>
	[EnumMember(Value = "ClientAddress")]
	ClientAddress = 2,

	/// <summary>
	/// Enum Confidential for value: Confidential
	/// </summary>
	[EnumMember(Value = "Confidential")]
	Confidential = 3,

	/// <summary>
	/// Enum StrictlyConfidential for value: StrictlyConfidential
	/// </summary>
	[EnumMember(Value = "StrictlyConfidential")]
	StrictlyConfidential = 4

}
