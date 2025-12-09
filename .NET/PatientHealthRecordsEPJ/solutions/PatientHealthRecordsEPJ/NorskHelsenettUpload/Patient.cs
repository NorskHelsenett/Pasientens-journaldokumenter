using NorskHelsenettUpload.Ihe.Mhd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorskHelsenettUpload;

public class Patient
{
	public SourcePatientInfo? SourcePatientInfo { get; set; }

	public bool HasPatient => SourcePatientInfo != null;

	public Patient WithName(PersonName Name)
	{
		if (SourcePatientInfo == null)
			SourcePatientInfo = new SourcePatientInfo();

		SourcePatientInfo.Name = Name;

		return this;
	}

	public Patient WithDateOfBirth(DateTime dateOfBirth)
	{
		if (SourcePatientInfo == null)
			SourcePatientInfo = new SourcePatientInfo();

		SourcePatientInfo.DateOfBirth= dateOfBirth;

		return this;
	}

	public Patient WithGender(string gender)
	{
		if (SourcePatientInfo == null)
			SourcePatientInfo = new SourcePatientInfo();

		SourcePatientInfo.Gender = gender;

		return this;
	}

	public Patient WithNIN(string nin)
	{
		if (SourcePatientInfo == null)
			SourcePatientInfo = new SourcePatientInfo();

		SourcePatientInfo.NIN = nin;

		return this;
	}

	public Patient WithNinType(string ninType)
	{
		if (SourcePatientInfo == null)
			SourcePatientInfo = new SourcePatientInfo();

		SourcePatientInfo.NinType = ninType;

		return this;
	}

	public Patient WithXdsGlobalIdentifier(string xdsGlobalIdentifier)
	{
		if (SourcePatientInfo == null)
			SourcePatientInfo = new SourcePatientInfo();

		SourcePatientInfo.XdsGlobalIdentifier = xdsGlobalIdentifier;

		return this;
	}

	public Patient WithXdsHomeCommunity(string xdsHomeCommunity)
	{
		if (SourcePatientInfo == null)
			SourcePatientInfo = new SourcePatientInfo();

		SourcePatientInfo.XdsHomeCommunity = xdsHomeCommunity;

		return this;
	}	
}
