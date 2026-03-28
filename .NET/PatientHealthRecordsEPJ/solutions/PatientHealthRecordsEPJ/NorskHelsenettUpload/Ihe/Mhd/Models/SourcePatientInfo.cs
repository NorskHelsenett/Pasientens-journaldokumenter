namespace NorskHelsenettUpload.Ihe.Mhd.Models
{
    public class NinType
    {
		public const string NinTypeNationalIdentityNumber = "NationalIdentityNumber";
		public const string NinTypeDnumber = "DNumber";
	}

	public class NinSystem
	{
		public const string NinSystemNationalIdentityNumber = "urn:oid:2.16.578.1.12.4.1.4.1";
		public const string NinSystemDnumber = "urn:oid:2.16.578.1.12.4.1.4.2";
	}

	public class SourcePatientInfo
    {        
		public PersonName Name { get; set; } = new(); // PID-5 => "PID-5|Family_name^First_name^Middle_name^^^"
        public DateTime DateOfBirth { get; set; } //PID-7 => "PID-7|19650715"
        public string? Gender { get; set; } // PID-8 => "PID-8|M"
        public string? NIN { get; set; } // Personal identifier
        public string? NinType { get; set; } // F-number or others (D-number, FHNumber), value: "DNumber" | "NationalIdentityNumber"
		public string? XdsGlobalIdentifier { get; set; } // Unique global identifier from XDS-environment
        public string? XdsHomeCommunity { get; set; } // Known OID by XDS-environmemt

        //PID-3 "PID-3|89765a87b^^^&3.4.5&ISO" //SourcePatientId
        //PID-11 Address "PID-11|100 Main St^^Metropolis^Il^44130^USA"        
    }
}


