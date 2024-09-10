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

		public static List<AuthorizationPair> AuthorizationList { get; set; } = new List<AuthorizationPair>()
		{
			new AuthorizationPair() { Id = "AA", Label = "Ambulansearbeider"},
			new AuthorizationPair() { Id = "AT", Label="Apotektekniker"},
			new AuthorizationPair() { Id = "AU", Label="Audiograf"},
			new AuthorizationPair() { Id = "BI", Label="Bioingeniør" },
			new AuthorizationPair() { Id = "ET", Label="Ergoterapeut"},
			new AuthorizationPair() { Id = "FB", Label="Fiskehelsebiolog"},
			new AuthorizationPair() { Id = "FO", Label="Fotterapeut"},
			new AuthorizationPair() { Id = "FT", Label="Fysioterapeut"},
			new AuthorizationPair() { Id = "HF", Label="Helsefagarbeider"},
			new AuthorizationPair() { Id = "HE", Label="Helsesekretær"},
			new AuthorizationPair() { Id = "HP", Label="Hjelpepleier"},
			new AuthorizationPair() { Id = "JO", Label="Jordmor"},
			new AuthorizationPair() { Id = "KI", Label="Kiropraktor"},
			new AuthorizationPair() { Id = "KE", Label="Klinisk ernæringsfysiolog"},
			new AuthorizationPair() { Id = "LE", Label="Lege"},
			new AuthorizationPair() { Id = "MT", Label="Manuellterapeut"},
			new AuthorizationPair() { Id = "NP", Label="Naprapat"},
			new AuthorizationPair() { Id = "OA", Label="Omsorgsarbeider"},
			new AuthorizationPair() { Id = "OP", Label="Optiker"},
			new AuthorizationPair() { Id = "OI", Label="Ortopediingeniør"},
			new AuthorizationPair() { Id = "OR", Label="Ortoptist"},
			new AuthorizationPair() { Id = "OS", Label="Osteopat"},
			new AuthorizationPair() { Id = "PM", Label="Paramedisiner"},
			new AuthorizationPair() { Id = "PE", Label="Perfusjonist"},
			new AuthorizationPair() { Id = "FA1", Label="Provisorfarmasøyt"},
			new AuthorizationPair() { Id = "PS", Label="Psykolog"},
			new AuthorizationPair() { Id = "RA", Label="Radiograf"},
			new AuthorizationPair() { Id = "FA2", Label="Reseptarfarmasøyt"},
			new AuthorizationPair() { Id = "SP", Label="Sykepleier"},
			new AuthorizationPair() { Id = "TH", Label="Tannhelsesekretær"},
			new AuthorizationPair() { Id = "TL", Label="Tannlege"},
			new AuthorizationPair() { Id = "TP", Label="Tannpleier"},
			new AuthorizationPair() { Id = "TT", Label="Tanntekniker"},
			new AuthorizationPair() { Id = "XX", Label="Ukjent/uspesifisert"},
			new AuthorizationPair() { Id = "VP", Label="Vernepleier"},
			new AuthorizationPair() { Id = "VE", Label="Veterinær"},

		};

		public static List<OrganizationPair> OrganizationList { get; set; } = new List<OrganizationPair>()
		{		
			new OrganizationPair() { Id = "BETANIEN_HOSPITAL" , Label="Betanien Hospital" },
			new OrganizationPair() { Id = "BJØLSENHJEMMET" , Label="Bjølsenhjemmet"},
			new OrganizationPair() { Id = "HAUKELAND_UNIVERSITETSSJUKEHUS" , Label="Haukeland Universitetssjukehus"},
			new OrganizationPair() { Id = "HELSE_BERGEN_HF_BERGEN_PSYKIATRISKE_LEGEVAKT" , Label="Helse Bergen HF Bergen Psykiatriske Legevakt"},
			new OrganizationPair() { Id = "HELSE_BERGEN_HF_VOSS" , Label="Helse Bergen HF Voss Sjukehus"},
			new OrganizationPair() { Id = "LODDEFJORD_LEGESENTER_AS" , Label="Loddefjord Legesenter"},
			new OrganizationPair() { Id = "MIDTBYEN_LEGESENTER_KONGSVINGER_DA" , Label="Midtbyen Legesenter Kongsvinger DA"},
			new OrganizationPair() { Id = "OSLO_UNIVERSITETSSYKEHUS_HF_OSLO_LEGEVAKT" , Label="Oslo Universitetssykehus HF Oslo Legevakt"},
			new OrganizationPair() { Id = "OSLO_UNIVERSITETSSYKEHUS_HF_RIKSHOSPITALET_SOMATIKK" , Label="Oslo Universitetssykehus HF Rikshospitalet - Somatikk"},
			new OrganizationPair() { Id = "OSLO_UNIVERSITETSSYKEHUS_HF_ULLEVAAL_SOMATIKK" , Label="Oslo Universitetssykehus HF Ullevål - Somatikk"},
			new OrganizationPair() { Id = "STAVANGER_LEGEVAKT_AVD_ARMAUER_HANSENS_VEI" , Label="Stavanger Legevakt Avd Armauer Hansens Vei"},
			new OrganizationPair() { Id = "SYKEHUSET_TELEMARK_HF" , Label="Sykehuset Telemark HF"},
			new OrganizationPair() { Id = "SØRLANDET_SYKEHUS_HF" , Label="Sørlandet Sykehus HF"},
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

	public class AuthorizationPair
	{
		public string Id { get; set; } = default!; 
		public string Label { get; set; } = default!;
	}

	public class OrganizationPair
	{
		public string Id { get; set; } = default!;
		public string Label { get; set; } = default!;
	}

	public class LegalEntityAndPointOfCarePair
	{
        public string Id { get; set; } = default!;
		public string LegalEntity { get; set; } = default!;
		public string PointOfCare { get; set; } = default!;
	}	
}

