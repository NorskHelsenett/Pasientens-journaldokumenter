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
#if false
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
					PurposeOfUse = TREAT,
					Lookup = new DokumentlagerTestCaseLookup()
					{
						LookupDokumentlager = new Lookup()
						{							
							RetrieveDocumentReferenceExpectVisible = true,							
							OpenDocumentExpectSuccess = true,
						},
						LookupAPI = new Lookup()
						{							
							RetrieveDocumentReferenceExpectVisible = false, // False for NORS							
							OpenDocumentExpectSuccess = false,	// Not possible to open document with NORS
						},
						LookupKJ = new Lookup()
						{							
							RetrieveDocumentReferenceExpectVisible = false, // False for NORS							
							OpenDocumentExpectSuccess = false,	// Not possible to open document with NORS
						},
						LookupHelsenorge = new Lookup()
						{							
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true,
						}
					}
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
						LookupDokumentlager = new Lookup()
						{							
							RetrieveDocumentReferenceExpectVisible = true, 							
							OpenDocumentExpectSuccess = true
						},
						LookupAPI = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupKJ = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupHelsenorge = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
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
					PurposeOfUse = TREAT,
					Lookup = new DokumentlagerTestCaseLookup()
					{
						LookupDokumentlager = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupAPI = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupKJ = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupHelsenorge = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = false,
							OpenDocumentExpectSuccess = false
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
					PurposeOfUse = TREAT,
					Lookup = new DokumentlagerTestCaseLookup()
					{
						LookupDokumentlager = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupAPI = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupKJ = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupHelsenorge = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = false,
							OpenDocumentExpectSuccess = false
						}
					}
				},
#endif
				new DokumentlagerTestCase
				{
					//Name = "Synlig referanse for HP, sperret referanse for pasient, kan åpnes av HP",
					Name = "Person dør. Ved død utleveres dokumentliste og journaldokument i 20 dager. Alle journaldokument og dokumentreferanser skal slettes etter 30 dager.\n " +
					"Sjekker at sletting av dokumenter blir planlagt 30 dager frem i tid.",
					ExpectedResult = "Failure",
					Comment = "",
					Details = "",
					SecurityLabels = new List<string>()
					{
						Normal
					},
					PurposeOfUse = TREAT,
					Lookup = new DokumentlagerTestCaseLookup()
					{
						LookupDokumentlager = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupAPI = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupKJ = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupHelsenorge = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						}
					},
					//Lookup = null,
					Events = new DokumentlagerTestEvents()
					{
						IsTrySimulateDeath = true						
					},
					//LookupAfterEvents = new DokumentlagerTestCaseLookup()
					//{

					//}
				},
				new DokumentlagerTestCase
				{
					// Dersom confidentiality code V (HL7) og NORN_ANG(9603) så slettes dokumentreferanse og journaldokument.

					//Name = "Synlig referanse for HP, sperret referanse for pasient, kan åpnes av HP",
					Name = "Person får adressesbeskyttelse med <em>Fortrolig</em> eller <em>Strengt fortrolig</em>.\n" +
					"Dersom journaldokumentet kun er delt med helsepersonell, med kode V (HL7) og NORN_ANG (9604), så skal det slettes umiddelbart.\n",
					//"Sjekker at dokumentreferanse og journaldokument blir slettet.",
					ExpectedResult = "Failure",
					Comment = "",
					Details = "",
					SecurityLabels = new List<string>()
					{
						VeryRestricted, NORN_ANG
					},
					PurposeOfUse = TREAT,
					Lookup = new DokumentlagerTestCaseLookup() // For V NORN_ANG (only visible to health personell, not visible in Helsenorge for patient)
					{
						LookupDokumentlager = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupAPI = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupKJ = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						},
						LookupHelsenorge = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = false,
							OpenDocumentExpectSuccess = false
						}
					},
					//Lookup = null,
					Events = new DokumentlagerTestEvents()
					{						
						IsTrySimulateAddressProtection = true
					},
					LookupAfterEvents = new DokumentlagerTestCaseLookup() // Lookup after setting address protection (and delete document due to V NORN_ANG) 
					{
						LookupDokumentlager = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = false,	// document should be completely deleted 
							OpenDocumentExpectSuccess = false
						},
						LookupAPI = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = false,
							OpenDocumentExpectSuccess = false
						},
						LookupKJ = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = false,
							OpenDocumentExpectSuccess = false
						},
						LookupHelsenorge = new Lookup()
						{
							RetrieveDocumentReferenceExpectVisible = false,
							OpenDocumentExpectSuccess = false
						}
					},
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

