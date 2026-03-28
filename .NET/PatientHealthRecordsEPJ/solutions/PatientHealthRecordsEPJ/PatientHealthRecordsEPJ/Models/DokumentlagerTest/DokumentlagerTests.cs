using PatientHealthRecordsEPJ.Personverninnstillinger;

using static NorskHelsenettUpload.Constants.CodeSystems.Hl7.ConfidentialityCode;
using static NorskHelsenettUpload.Constants.CodeSystems.Volven.ConfidentialityCode;
using static NorskHelsenettUpload.Constants.CodeSystems.Hl7.PurposeOfUse; 

namespace PatientHealthRecordsEPJ.Models;

public class DokumentlagerTests
{
	public List<DokumentlagerTestGroup> TestGroups { get; set; } = new List<DokumentlagerTestGroup>
	{
		new DokumentlagerTestGroup
		{
			Name = "Del dokument",
			TestCases = new List<DokumentlagerTestCase>
			{
				new DokumentlagerTestCase
				{
					Name = "Journaldokument skal vises på Helsenorge, men ikke til helsepersonell. Kan åpnes på Helsenorge.",
					ExpectedResult = "Failure",
					Comment = "",
					Details = "",
					SecurityLabels = new List<string>()
					{
						Normal, NORS
					},
					PurposeOfUse = TREAT
				},
				new DokumentlagerTestCase
				{
					Name = "Journaldokument skal vises på Helsenorge og til helsepersonell. Kan åpnes av helsepersonell og på Helsenorge.",
					ExpectedResult = "Success",
					Comment = "",
					Details = "",
					SecurityLabels = new List<string>()
					{
						Normal
					},
					PurposeOfUse = TREAT,
					Lookup = new DokumentlagerTestCaseLookup()
					{						
						LookupAPI = new Lookup()
						{							 
						},
						LookupKJ = new Lookup()
						{
						},
						LookupHelsenorge = new Lookup()
						{							
							RetrieveDocumentReferenceExpectVisibleWithObfuscation = true
						}
					}
				},
				
				new DokumentlagerTestCase
				{
					//Name = "Synlig referanse for HP, sperret referanse for pasient, kan åpnes av HP",
					Name = "Journaldokumentet skal vises til helsepersonell og ikke på Helsenorge. Kan åpnes av HP.",
					ExpectedResult = "Failure",
					Comment = "",
					Details = "",
					SecurityLabels = new List<string>()
					{
						VeryRestricted, NORN_ANG
					},
					PurposeOfUse = TREAT
				}				
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Oppslag i Kjernejournal",
			TestCases = new List<DokumentlagerTestCase>
			{
				
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Helsenorge",
			TestCases = new List<DokumentlagerTestCase>
			{				
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Tilgangslogg",
			TestCases = new List<DokumentlagerTestCase>
			{
				
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Auditlogg",
			TestCases = new List<DokumentlagerTestCase>
			{
				
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Tekniske tester",
			TestCases = new List<DokumentlagerTestCase>
			{				
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Brukstall",
			TestCases = new List<DokumentlagerTestCase>
			{				
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Metadata",
			TestCases = new List<DokumentlagerTestCase>
			{
				
			}
		}
	};
}

