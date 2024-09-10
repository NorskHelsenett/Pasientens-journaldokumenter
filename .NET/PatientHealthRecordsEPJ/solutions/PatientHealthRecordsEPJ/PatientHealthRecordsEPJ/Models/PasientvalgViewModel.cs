namespace PatientHealthRecordsEPJ.Models
{

	public class EnvironmentPair
	{
		public string Id { get; set; } = default!;
		public string Label { get; set; } = default!;
	}

	public class GrunnlagPair
	{
		public string Id { get; set; } = default!;
		public string Label { get; set; } = default!;
	}

	public class PatientPair
	{
		public string Id { get; set; } = default!;
		public string Name { get; set; } = default!;
	}

	public class PasientvalgViewModel
	{		
		public string PatientId { get; set; } = default!; 

		public string GrunnlagId { get; set; } = default!;
		public string GrunnlagName { get; set; } = default!;
		
		public string EnvironmentName { get; set; } = default!;
		public string EnvironmentId { get; set; } = default!;
	
		public static List<EnvironmentPair> EnvironmentList { get; set; } = new List<EnvironmentPair>()
		{
			new EnvironmentPair() { Id = "test" , Label="Test" },
		};

		public static List<GrunnlagPair> GrunnlagList { get; set; } = new List<GrunnlagPair>()
		{
			new GrunnlagPair() { Id = "akutt" , Label = "Akutt" },
			new GrunnlagPair() { Id = "samtykke" , Label = "Samtykke" },
			new GrunnlagPair() { Id = "unntak" , Label = "Unntak" },
		};

		public static List<PatientPair> PatientMappingList { get; set; } = new List<PatientPair>()
		{
			new PatientPair() { Id = "13116900216" , Name = "Line Danser"},
			new PatientPair() { Id = "12119000465" , Name = "Malin Fos Bogen"},
			new PatientPair() { Id = "08077000292" , Name = "Harry Handel"}			
		};
	}
}

