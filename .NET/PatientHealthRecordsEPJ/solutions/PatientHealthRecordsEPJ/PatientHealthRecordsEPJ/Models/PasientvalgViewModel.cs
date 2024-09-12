namespace PatientHealthRecordsEPJ.Models
{
	public class OptionPair
	{
		public string Id { get; set; } = default!;
		public string Label { get; set; } = default!;
	}

	public class PasientvalgViewModel
	{
		public string PatientId { get; set; } = default!;

		public string GrunnlagId { get; set; } = default!;
		public string GrunnlagName { get; set; } = default!;

		public string EnvironmentName { get; set; } = default!;
		public string EnvironmentId { get; set; } = default!;
		public static List<OptionPair> EnvironmentList { get; set; } = new List<OptionPair>()
		{
			new OptionPair() { Id = "test" , Label ="Test" },
		};

		public static List<OptionPair> DocumentListScopeList { get; set; } = new List<OptionPair>()
		{
			new OptionPair() { Id = ((int)DocumentListScopeEnum.HelsepersonellovenParagraf45).ToString(), Label = "Helsepersonelloven § 45 (krever ikke grunnlag/access-basis, ingen filtrering)" },
			new OptionPair() { Id = ((int)DocumentListScopeEnum.Kjernejournalforskriften).ToString(), Label = "Kjernejournalforskriften (krever grunnlag/access-basis, har filtrering)" },
		};

		public static List<OptionPair> GrunnlagList { get; set; } = new List<OptionPair>()
		{
			new OptionPair() { Id = "akutt" , Label = "Akutt" },
			new OptionPair() { Id = "samtykke" , Label = "Samtykke" },
			new OptionPair() { Id = "unntak" , Label = "Unntak" },
		};

		public static List<OptionPair> PatientMappingList { get; set; } = new List<OptionPair>()
		{
			new OptionPair() { Id = "13116900216" , Label = "Line Danser"},
			new OptionPair() { Id = "12119000465" , Label = "Malin Fos Bogen"},
			new OptionPair() { Id = "08077000292" , Label = "Harry Handel"}
		};
	}
}

