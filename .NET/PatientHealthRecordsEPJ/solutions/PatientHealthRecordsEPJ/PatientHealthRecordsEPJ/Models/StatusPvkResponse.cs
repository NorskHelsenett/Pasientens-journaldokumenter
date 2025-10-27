namespace PatientHealthRecordsEPJ.Models; 

public class StatusPvkResult
{
	public string PatientNIN { get; set; }
	
	public string? Kjernejournal_Reservasjon_Json { get; set; } // b39f778a-07e3-41ea-99c7-eb332ad45c82
	public string? Kjernejournal_Sperre_Tilgang_Alt_Helsepersonell_Json { get; set; } // 86f64955-fb61-4e5e-8a0c-7d7728363891
	public string? Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Json { get; set; } // 36870d8a-b37b-4386-8689-c656005cb296
	public string? Journaldokumenter_Sperre_Tilgang_Alt_Helsepersonell_Json { get; set; } // f0a455c5-c95f-4382-9cf0-4481a732a694	
	public string? Besokshistorikk_Blokkere_For_Besokshistorikk_og_Journaldokumenter_Json { get; set; } // c8ed0076-7482-4566-81f9-a41f2179b0ae

	public bool? Kjernejournal_Reservasjon_Active { get; set; }
	public bool? Kjernejournal_Sperre_Tilgang_Alt_Helsepersonell_Active { get; set; }
	public bool? Kjernejournal_Blokkere_For_Individuelt_Helsepersonell_Active { get; set; }
	public bool? Journaldokumenter_Sperre_Tilgang_Alt_Helsepersonell_Active { get; set; }	
	public bool? Besokshistorikk_Blokkere_For_Besokshistorikk_og_Journaldokumenter_Active { get; set; }	
}
