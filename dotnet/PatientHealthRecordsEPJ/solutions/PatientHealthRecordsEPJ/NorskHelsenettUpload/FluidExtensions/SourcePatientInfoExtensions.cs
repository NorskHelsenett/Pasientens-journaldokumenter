using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Models;

namespace NorskHelsenettUpload.FluidExtensions;

public static class SourcePatientInfoExtensions
{

	//
	//	Source patient info
	//
	//	
	public static SourcePatientInfo WithName(this SourcePatientInfo patientInfo, PersonName name)
	{
		patientInfo.Name = name;
		return patientInfo;
	}

	public static SourcePatientInfo WithDateOfBirth(this SourcePatientInfo patientInfo, DateTime dateOfBirth)
	{
		patientInfo.DateOfBirth = dateOfBirth;
		return patientInfo;
	}

	public static SourcePatientInfo WithGender(this SourcePatientInfo patientInfo, string gender)
	{
		patientInfo.Gender = gender;
		return patientInfo;
	}

	public static SourcePatientInfo WithNIN(this SourcePatientInfo patientInfo, string nin, string ninType)
	{
		patientInfo.NIN = nin;
		patientInfo.NinType = ninType;
		return patientInfo;
	}

	public static SourcePatientInfo WithNinType(this SourcePatientInfo patientInfo, string ninType)
	{
		patientInfo.NinType = ninType;
		return patientInfo;
	}

	public static SourcePatientInfo WithXdsGlobalIdentifier(this SourcePatientInfo patientInfo, string xdsGlobalIdentifier)
	{
		patientInfo.XdsGlobalIdentifier = xdsGlobalIdentifier;
		return patientInfo;
	}

	public static SourcePatientInfo WithXdsHomeCommunity(this SourcePatientInfo patientInfo, string xdsHomeCommunity)
	{
		patientInfo.XdsHomeCommunity = xdsHomeCommunity;
		return patientInfo;
	}




}

