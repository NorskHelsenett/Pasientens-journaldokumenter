using PatientHealthRecordsEPJ.Models.HelsenorgeLogin;
using PatientHealthRecordsEPJ.Personverninnstillinger;
using static NorskHelsenettUpload.Constants.CodeSystems.Hl7.ConfidentialityCode;
using static NorskHelsenettUpload.Constants.CodeSystems.Hl7.PurposeOfUse; 
using static NorskHelsenettUpload.Constants.CodeSystems.Volven.ConfidentialityCode;

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
					Name = "Journaldokument skal vises på Helsenorge, men ikke til helsepersonell. \nKan åpnes på Helsenorge.",
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
							RetrieveDocumentReferenceExpectVisibleWithObfuscation = true, // True for NORS
							OpenDocumentExpectSuccess = false,	// Not possible to open document with NORS
						},
						LookupKJ = new Lookup()
						{							
							RetrieveDocumentReferenceExpectVisible = false, // False for NORS
							RetrieveDocumentReferenceExpectVisibleWithObfuscation = true,
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
					Name = "Journaldokument skal vises på Helsenorge og til helsepersonell. \nKan åpnes av helsepersonell og på Helsenorge.",
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
					Name = "Journaldokumentet skal vises til helsepersonell og ikke på Helsenorge. \nKan åpnes av HP.",
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
				/*new DokumentlagerTestCase
				{
					//Name = "Synlig referanse for HP, sperret referanse for pasient, kan åpnes av HP",
					Name = "Journaldokumentet skal vises til helsepersonell og ikke på Helsenorge. \nKan åpnes av HP.",
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
				}, */								
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Pasient dør",
			TestCases = new List<DokumentlagerTestCase>
			{
				new DokumentlagerTestCase
				{
					Name = "Person dør. Ved død utleveres dokumentliste og journaldokument i 30 dager. Alle journaldokument og dokumentreferanser skal slettes etter 30 dager.\n " +
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
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Pasient får adressebeskyttelse",
			TestCases = new List<DokumentlagerTestCase>
			{
				new DokumentlagerTestCase
				{
					// Dersom confidentiality code V (HL7) og NORN_ANG(9603) så slettes dokumentreferanse og journaldokument.					
					//Name = "Person får adressesbeskyttelse med <em>Fortrolig</em> eller <em>Strengt fortrolig</em>.\n" +
					//"Dersom journaldokumentet kun er delt med helsepersonell, med kode V (HL7) og NORN_ANG (9604), så skal det slettes umiddelbart.\n",

					Name = "Person får adressesbeskyttelse med <em>Fortrolig</em> eller <em>Strengt fortrolig</em>.\n" +
					"Dokumentliste og journaldokument utleveres i 30 dager. Alle journaldokument og dokumentreferanser skal slettes etter 30 dager dersom adressebeskyttelsen ikke oppheves.\n " +
					"Sjekker at sletting av dokumenter blir planlagt 30 dager frem i tid.",
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
					/*LookupAfterEvents = new DokumentlagerTestCaseLookup() // Lookup after setting address protection (and delete document due to V NORN_ANG) 
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
					}, */
				}
			}
		},
		/*new DokumentlagerTestGroup
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
		},*/
		new DokumentlagerTestGroup
		{
			Name = "Barn, ungdom og foreldrerepresentasjon",
			TestCases = new List<DokumentlagerTestCase>
			{
				new DokumentlagerTestCase
				{
					Name = "18 åringer skal kunne se journaldokumenter tilbake i tid",
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
					Name = "Person mellom 16 og 18 kan se journaldokument og dokumentliste så lenge ikke V og NORN_ANG er satt",
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
					Name = "Person mellom 16 og 18 kan IKKE se journaldokument og dokumentliste når V og NORN_ANG er satt",
					ExpectedResult = "Success",
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
							RetrieveDocumentReferenceExpectVisible = true,
							OpenDocumentExpectSuccess = true
						}
					}
				},

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

