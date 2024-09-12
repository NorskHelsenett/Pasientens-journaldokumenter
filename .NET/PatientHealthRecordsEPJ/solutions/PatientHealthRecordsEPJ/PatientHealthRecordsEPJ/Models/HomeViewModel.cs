namespace PatientHealthRecordsEPJ.Models
{
	public class HomeViewModel
	{
		public string OrganizationName { get; set; } = default!;
		public string OrganizationId { get; set; } = default!;

		public string HealthcarePersonellUserId { get; set; } = default!; // Helsepersonell fnr

		public string AuthorizationName { get; set; } = default!;
		public string AuthorizationId { get; set; } = default!;
		
		public string AttestJson { get; set; } = default!;

		public string EnvironmentName { get; set; } = default!;
		public string EnvironmentId { get; set; } = default!;

		public int  DocumentListScopeId { get; set; } = default!; // See DocumentListScopeEnum

		public static List<OptionPair> AuthorizationList { get; set; } = new List<OptionPair>()
		{
			new OptionPair() { Id = "AA", Label = "Ambulansearbeider"},
			new OptionPair() { Id = "AT", Label="Apotektekniker"},
			new OptionPair() { Id = "AU", Label="Audiograf"},
			new OptionPair() { Id = "BI", Label="Bioingeniør" },
			new OptionPair() { Id = "ET", Label="Ergoterapeut"},
			new OptionPair() { Id = "FB", Label="Fiskehelsebiolog"},
			new OptionPair() { Id = "FO", Label="Fotterapeut"},
			new OptionPair() { Id = "FT", Label="Fysioterapeut"},
			new OptionPair() { Id = "HF", Label="Helsefagarbeider"},
			new OptionPair() { Id = "HE", Label="Helsesekretær"},
			new OptionPair() { Id = "HP", Label="Hjelpepleier"},
			new OptionPair() { Id = "JO", Label="Jordmor"},
			new OptionPair() { Id = "KI", Label="Kiropraktor"},
			new OptionPair() { Id = "KE", Label="Klinisk ernæringsfysiolog"},
			new OptionPair() { Id = "LE", Label="Lege"},
			new OptionPair() { Id = "MT", Label="Manuellterapeut"},
			new OptionPair() { Id = "NP", Label="Naprapat"},
			new OptionPair() { Id = "OA", Label="Omsorgsarbeider"},
			new OptionPair() { Id = "OP", Label="Optiker"},
			new OptionPair() { Id = "OI", Label="Ortopediingeniør"},
			new OptionPair() { Id = "OR", Label="Ortoptist"},
			new OptionPair() { Id = "OS", Label="Osteopat"},
			new OptionPair() { Id = "PM", Label="Paramedisiner"},
			new OptionPair() { Id = "PE", Label="Perfusjonist"},
			new OptionPair() { Id = "FA1", Label="Provisorfarmasøyt"},
			new OptionPair() { Id = "PS", Label="Psykolog"},
			new OptionPair() { Id = "RA", Label="Radiograf"},
			new OptionPair() { Id = "FA2", Label="Reseptarfarmasøyt"},
			new OptionPair() { Id = "SP", Label="Sykepleier"},
			new OptionPair() { Id = "TH", Label="Tannhelsesekretær"},
			new OptionPair() { Id = "TL", Label="Tannlege"},
			new OptionPair() { Id = "TP", Label="Tannpleier"},
			new OptionPair() { Id = "TT", Label="Tanntekniker"},
			new OptionPair() { Id = "XX", Label="Ukjent/uspesifisert"},
			new OptionPair() { Id = "VP", Label="Vernepleier"},
			new OptionPair() { Id = "VE", Label="Veterinær"},

		};

		public static List<OptionPair> OrganizationList { get; set; } = new List<OptionPair>()
		{		
			new OptionPair() { Id = "BETANIEN_HOSPITAL" , Label="Betanien Hospital" },
			new OptionPair() { Id = "BJØLSENHJEMMET" , Label="Bjølsenhjemmet"},
			new OptionPair() { Id = "HAUKELAND_UNIVERSITETSSJUKEHUS" , Label="Haukeland Universitetssjukehus"},
			new OptionPair() { Id = "HELSE_BERGEN_HF_BERGEN_PSYKIATRISKE_LEGEVAKT" , Label="Helse Bergen HF Bergen Psykiatriske Legevakt"},
			new OptionPair() { Id = "HELSE_BERGEN_HF_VOSS" , Label="Helse Bergen HF Voss Sjukehus"},
			new OptionPair() { Id = "LODDEFJORD_LEGESENTER_AS" , Label="Loddefjord Legesenter"},
			new OptionPair() { Id = "MIDTBYEN_LEGESENTER_KONGSVINGER_DA" , Label="Midtbyen Legesenter Kongsvinger DA"},
			new OptionPair() { Id = "OSLO_UNIVERSITETSSYKEHUS_HF_OSLO_LEGEVAKT" , Label="Oslo Universitetssykehus HF Oslo Legevakt"},
			new OptionPair() { Id = "OSLO_UNIVERSITETSSYKEHUS_HF_RIKSHOSPITALET_SOMATIKK" , Label="Oslo Universitetssykehus HF Rikshospitalet - Somatikk"},
			new OptionPair() { Id = "OSLO_UNIVERSITETSSYKEHUS_HF_ULLEVAAL_SOMATIKK" , Label="Oslo Universitetssykehus HF Ullevål - Somatikk"},
			new OptionPair() { Id = "STAVANGER_LEGEVAKT_AVD_ARMAUER_HANSENS_VEI" , Label="Stavanger Legevakt Avd Armauer Hansens Vei"},
			new OptionPair() { Id = "SYKEHUSET_TELEMARK_HF" , Label="Sykehuset Telemark HF"},
			new OptionPair() { Id = "SØRLANDET_SYKEHUS_HF" , Label="Sørlandet Sykehus HF"},
		};

		public static List<LegalEntityAndPointOfCarePair> OrganizationMappingList { get; set; } = new List<LegalEntityAndPointOfCarePair>()
		{
			new LegalEntityAndPointOfCarePair() { Id = "BETANIEN_HOSPITAL" , LegalEntity = "981275721", PointOfCare = "873255102" },
			new LegalEntityAndPointOfCarePair() { Id = "BJØLSENHJEMMET" , LegalEntity = "958935420", PointOfCare = "883953142" },
			new LegalEntityAndPointOfCarePair() { Id = "HAUKELAND_UNIVERSITETSSJUKEHUS" , LegalEntity = "983974724", PointOfCare = "974557746" },
			new LegalEntityAndPointOfCarePair() { Id = "HELSE_BERGEN_HF_BERGEN_PSYKIATRISKE_LEGEVAKT" , LegalEntity = "983974724", PointOfCare = "923822488" },
			new LegalEntityAndPointOfCarePair() { Id = "HELSE_BERGEN_HF_VOSS" , LegalEntity = "983974724", PointOfCare = "974743272" },
			new LegalEntityAndPointOfCarePair() { Id = "LODDEFJORD_LEGESENTER_AS" , LegalEntity = "915638449", PointOfCare = "974870231" },
			new LegalEntityAndPointOfCarePair() { Id = "MIDTBYEN_LEGESENTER_KONGSVINGER_DA" , LegalEntity = "994919806", PointOfCare = "994945688" },
			new LegalEntityAndPointOfCarePair() { Id = "OSLO_UNIVERSITETSSYKEHUS_HF_OSLO_LEGEVAKT" , LegalEntity = "993467049", PointOfCare = "974589087" },
			new LegalEntityAndPointOfCarePair() { Id = "OSLO_UNIVERSITETSSYKEHUS_HF_RIKSHOSPITALET_SOMATIKK" , LegalEntity = "993467049", PointOfCare = "874716782" },
			new LegalEntityAndPointOfCarePair() { Id = "OSLO_UNIVERSITETSSYKEHUS_HF_ULLEVAAL_SOMATIKK" , LegalEntity = "993467049", PointOfCare = "974589095" },
			new LegalEntityAndPointOfCarePair() { Id = "STAVANGER_LEGEVAKT_AVD_ARMAUER_HANSENS_VEI" , LegalEntity = "874766852", PointOfCare = "874611972" },
			new LegalEntityAndPointOfCarePair() { Id = "SYKEHUSET_TELEMARK_HF" , LegalEntity = "983975267", PointOfCare = "974633183" },
			new LegalEntityAndPointOfCarePair() { Id = "SØRLANDET_SYKEHUS_HF" , LegalEntity = "983975240", PointOfCare = "974733013" },			
		};	
	}
	
	public class LegalEntityAndPointOfCarePair
	{
        public string Id { get; set; } = default!;
		public string LegalEntity { get; set; } = default!;
		public string PointOfCare { get; set; } = default!;
	}	
}

