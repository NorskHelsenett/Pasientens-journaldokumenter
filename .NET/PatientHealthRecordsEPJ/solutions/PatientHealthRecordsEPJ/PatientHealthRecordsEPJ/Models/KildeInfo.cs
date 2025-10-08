namespace PatientHealthRecordsEPJ.Models;

public class KildeInfo
{
	public enum KildeTypeEnum
	{
		SiemensXds = 0,
		NasjonaltLager = 1	// Applikasjon instans i privat sky
	}

	public string Name { get; set; } = default!;
	public string HomeCommunityId { get; set; } = default!;	// SourceId i submission set points to this
	public string AppName { get; set; } = default!;
	public string Url { get; set; } = default!; // Default path is "/Repository/services/RepositoryService"

	public KildeTypeEnum KildeType { get; set; } = default!; // 
	
	public static List<KildeInfo> KildeList { get; set; } = new List<KildeInfo>()
	{
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.15", Name = "Bjarne sykehus", AppName = "bjarne-sykehus", Url = "https://bjarne-sykehus.t-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.17", Name = "Kurt sykehus", AppName = "kurt-sykehus", Url = "https://kurt-sykehus.t-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.18", Name = "Rex sykehus", AppName = "rex-sykehus", Url = "https://rex-sykehus.t-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},
	};	
}