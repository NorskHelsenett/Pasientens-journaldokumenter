using NorskHelsenettUpload.Ihe.Mhd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorskHelsenettUpload; 

public class Classification
{
	public string? HealthcareFacilityType { get; set; }

	public Classification WithHealthcareFacilityType(string healthcareFacilityType)
	{
		//if (Classification == null)
		//	Classification = new Classification();

		HealthcareFacilityType = healthcareFacilityType;

		return this;
	}
}
