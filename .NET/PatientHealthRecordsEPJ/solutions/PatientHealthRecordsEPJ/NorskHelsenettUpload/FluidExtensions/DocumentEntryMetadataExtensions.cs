using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Models;

namespace NorskHelsenettUpload.FluidExtensions;

public static class DocumentEntryMetadataExtensions
{
	
	//
	//	Document metadata
	//
	
	//private static SafeSystemCode

	public static DocumentEntryMetadata WithUniqueId(this DocumentEntryMetadata metadata, Guid uniqueId)
	{
		metadata.UniqueId = uniqueId; 
		return metadata;
	}

	public static DocumentEntryMetadata WithPatientId(this DocumentEntryMetadata metadata, string patientId)
	{
		metadata.PatientId = patientId;
		return metadata;
	}

	public static DocumentEntryMetadata WithSourcePatientIdToHl7String(this DocumentEntryMetadata metadata, string patientNin, string ninType)
	{
		var identifierType = ninType switch
		{
			NinType.NinTypeDnumber => "2.16.578.1.12.4.1.4.2",
			NinType.NinTypeNationalIdentityNumber => "2.16.578.1.12.4.1.4.1",
			_ => ""
		};

		//metadata.SourcePatientId = sourcePatientId;
		metadata.SourcePatientId = $"{patientNin}^^^&amp;{identifierType}&amp;ISO";
		return metadata;
	}

	public static DocumentEntryMetadata WithLegalAuthenticator(this DocumentEntryMetadata metadata, Xcn legalAuthenticator)
	{
		metadata.LegalAuthenticator = legalAuthenticator;
		return metadata;
	}

	public static DocumentEntryMetadata WithServiceStartTime(this DocumentEntryMetadata metadata, DateTime serviceStartTime)
	{
		metadata.ServiceStartTime = serviceStartTime;
		return metadata;
	}

	public static DocumentEntryMetadata WithServiceStopTime(this DocumentEntryMetadata metadata, DateTime serviceStopTime)
	{
		metadata.ServiceStopTime = serviceStopTime;
		return metadata;
	}

	public static DocumentEntryMetadata WithSourcePatientInfo(this DocumentEntryMetadata metadata, SourcePatientInfo? sourcePatientInfo)
	{
		metadata.SourcePatientInfo = sourcePatientInfo; 
		return metadata;
	}

	//public static DocumentEntryMetadata WithClassCode(this DocumentEntryMetadata metadata, string classCode)
	public static DocumentEntryMetadata WithClassCode(this DocumentEntryMetadata metadata, SimpleCoding classCode)
	{		
		metadata.ClassCode = classCode.ToValueString(); 
		return metadata;
	}

	public static DocumentEntryMetadata WithConfidentialityCode(this DocumentEntryMetadata metadata, SimpleCoding confidentialityCode)
	{
		metadata.ConfidentialityCodeList.Add(confidentialityCode.ToValueString());
		return metadata;
	}

	public static DocumentEntryMetadata WithFormatCode(this DocumentEntryMetadata metadata, string? formatCode)
	{
		metadata.FormatCode = formatCode;
		return metadata;
	}

	public static DocumentEntryMetadata WithHealthCareFacilityType(this DocumentEntryMetadata metadata, string healthCareFacilityType)
	{
		metadata.HealthCareFacilityType = healthCareFacilityType;
		return metadata;
	}

	public static DocumentEntryMetadata WithPracticeSettingCode(this DocumentEntryMetadata metadata, string practiceSettingCode)
	{
		metadata.PracticeSettingCode = practiceSettingCode;
		return metadata;
	}

	public static DocumentEntryMetadata WithTypeCode(this DocumentEntryMetadata metadata, SimpleCoding typeCode)
	{
		metadata.TypeCode = typeCode.ToValueString(); 
		return metadata;
	}

	public static DocumentEntryMetadata WithEventCode(this DocumentEntryMetadata metadata, string eventCode)
	{
		if (metadata.EventCodeList == null)
			metadata.EventCodeList = []; 

		metadata.EventCodeList.Add(eventCode);
		return metadata;
	}

	public static DocumentEntryMetadata WithAuthor(this DocumentEntryMetadata metadata, Author author)
	{
		metadata.Authors.Add(author); 
		return metadata;
	}

	public static DocumentEntryMetadata WithComment(this DocumentEntryMetadata metadata, string comment)
	{
		metadata.Comment = comment;
		return metadata;
	}	
}

