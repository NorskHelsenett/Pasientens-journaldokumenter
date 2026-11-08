using System;

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
	public bool IsShowInUploadWizard { get; set; } = true;

	public KildeTypeEnum KildeType { get; set; } = default!; // 

	// OID structure includes (z) for environment: 
	// home: 2.16.578.1.12.4.5.100.z.xx
	// repository: 2.16.578.1.12.4.5.100.z.xx.yy
	// 1 for Prod, 2 for QA, 3 for Test og 4 for DevTest

	public static List<KildeInfo> KildeListDev { get; set; } = new List<KildeInfo>()
	{
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.1.1", Name = "Local development", AppName = "local-development-sykehus", Url = "", KildeType = KildeTypeEnum.NasjonaltLager, IsShowInUploadWizard = false },
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.4.15", Name = "Aktivt sykehus", AppName = "aktivt-sykehus", Url = "https://viti-aktivt-sykehus.d-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.4.16", Name = "Travelt sykehus", AppName = "travelt-sykehus", Url = "https://viti-travelt-sykehus.d-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.4.17", Name = "Stort sykehus", AppName = "stort-sykehus", Url = "https://viti-stort-sykehus.d-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},
	};

	public static List<KildeInfo> KildeListTest { get; set; } = new List<KildeInfo>()
	{		
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.1.1", Name = "Local development", AppName = "local-development-sykehus", Url = "", KildeType = KildeTypeEnum.NasjonaltLager, IsShowInUploadWizard = false },
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.3.15", Name = "Bjarne sykehus", AppName = "bjarne-sykehus", Url = "https://bjarne-sykehus.t-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.3.16", Name = "Tim sykehus", AppName = "tim-sykehus", Url = "https://tim-sykehus.t-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.3.17", Name = "Kurt sykehus", AppName = "kurt-sykehus", Url = "https://kurt-sykehus.t-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.3.18", Name = "Rex sykehus", AppName = "rex-sykehus", Url = "https://rex-sykehus.t-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},		
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.3.19", Name = "Origo sykehus", AppName = "origo-sykehus", Url = "https://origo-sykehus.t-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager, IsShowInUploadWizard = false},
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.3.20", Name = "Pentest sykehus", AppName = "pentest-sykehus", Url = "https://pentest-sykehus.t-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager, IsShowInUploadWizard = false},
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.3.21", Name = "DocY", AppName = "docy", Url = "https://viti-repo-docy.t-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager, IsShowInUploadWizard = false},
	};

	public static List<KildeInfo> KildeListQA { get; set; } = new List<KildeInfo>()
	{
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.1.1", Name = "Local development", AppName = "local-development-sykehus", Url = "", KildeType = KildeTypeEnum.NasjonaltLager, IsShowInUploadWizard = false },
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.2.15", Name = "Bjarne sykehus", AppName = "bjarne-sykehus", Url = "https://bjarne-sykehus.q-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.2.16", Name = "Tim sykehus", AppName = "tim-sykehus", Url = "https://tim-sykehus.q-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.2.17", Name = "Kurt sykehus", AppName = "kurt-sykehus", Url = "https://kurt-sykehus.q-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},
		new KildeInfo() { HomeCommunityId = "2.16.578.1.12.4.5.100.2.18", Name = "Rex sykehus", AppName = "rex-sykehus", Url = "https://rex-sykehus.q-xcads.pjd.nhn.no", KildeType = KildeTypeEnum.NasjonaltLager},
	};
}