#if false
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
					Name = "Synlig referanse for HP og pasient, kan åpnes av HP og pasient",
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
					Name = "Synlig referanse for pasient, kan åpnes av pasient",
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
					Name = "Synlig referanse for HP, sperret referanse for pasient, kan åpnes av HP",
					ExpectedResult = "Failure",
					Comment = "",
					Details = "",
					SecurityLabels = new List<string>()
					{
						Normal, NORN_ANG
					},
					PurposeOfUse = TREAT
				},
				new DokumentlagerTestCase
				{
					Name = "Test Case 1",
					ExpectedResult = "Success",
					Comment = "",
					Details = "",
					SecurityLabels = new List<string>()
					{
						Restricted
					},
					PurposeOfUse = TREAT
				},
				new DokumentlagerTestCase
				{
					Name = "Test Case 2",
					ExpectedResult = "Failure",
					Comment = "",
					Details = "",
					SecurityLabels = new List<string>()
					{
						Restricted, NORS
					},
					PurposeOfUse = TREAT
				},
				new DokumentlagerTestCase
				{
					Name = "Test Case 2",
					ExpectedResult = "Failure",
					Comment = "",
					Details = "",
					SecurityLabels = new List<string>()
					{
						Restricted, NORN_ANG
					},
					PurposeOfUse = TREAT
				},
				new DokumentlagerTestCase
				{
					Name = "Test Case 1",
					ExpectedResult = "Success",
					Comment = "",
					Details = "",
					SecurityLabels = new List<string>()
					{
						VeryRestricted
					},
					PurposeOfUse = TREAT
				},
				new DokumentlagerTestCase
				{
					Name = "Test Case 2",
					ExpectedResult = "Failure",
					Comment = "",
					Details = "",
					SecurityLabels = new List<string>()
					{
						VeryRestricted, NORS
					},
					PurposeOfUse = TREAT
				},
				new DokumentlagerTestCase
				{
					Name = "Test Case 2",
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
				new DokumentlagerTestCase
				{
					Name = "Test Case 3",
					ExpectedResult = "Success",
					Comment = "",
					Details = "",
				},
				new DokumentlagerTestCase
				{
					Name = "Test Case 4",
					ExpectedResult = "Failure",
					Comment = "",
					Details = "",
				}
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Helsenorge",
			TestCases = new List<DokumentlagerTestCase>
			{
				new DokumentlagerTestCase
				{
					Name = "Test Case 3",
					ExpectedResult = "Success",
					Comment = "",
					Details = "",
				},
				new DokumentlagerTestCase
				{
					Name = "Test Case 4",
					ExpectedResult = "Failure",
					Comment = "",
					Details = "",
				}
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Tilgangslogg",
			TestCases = new List<DokumentlagerTestCase>
			{
				new DokumentlagerTestCase
				{
					Name = "Test Case 3",
					ExpectedResult = "Success",
					Comment = "",
					Details = "",
				},
				new DokumentlagerTestCase
				{
					Name = "Test Case 4",
					ExpectedResult = "Failure",
					Comment = "",
					Details = "",
				}
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Auditlogg",
			TestCases = new List<DokumentlagerTestCase>
			{
				new DokumentlagerTestCase
				{
					Name = "Test Case 3",
					ExpectedResult = "Success",
					Comment = "",
					Details = "",
				},
				new DokumentlagerTestCase
				{
					Name = "Test Case 4",
					ExpectedResult = "Failure",
					Comment = "",
					Details = "",
				}
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Tekniske tester",
			TestCases = new List<DokumentlagerTestCase>
			{
				new DokumentlagerTestCase
				{
					Name = "Test Case 3",
					ExpectedResult = "Success",
					Comment = "",
					Details = "",
				},
				new DokumentlagerTestCase
				{
					Name = "Test Case 4",
					ExpectedResult = "Failure"					
				}
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Brukstall",
			TestCases = new List<DokumentlagerTestCase>
			{
				new DokumentlagerTestCase
				{
					Name = "Test Case 3",
					ExpectedResult = "Success",					
				},
				new DokumentlagerTestCase
				{
					Name = "Test Case 4",
					ExpectedResult = "Failure"
				}
			}
		},
		new DokumentlagerTestGroup
		{
			Name = "Metadata",
			TestCases = new List<DokumentlagerTestCase>
			{
				new DokumentlagerTestCase
				{
					Name = "Test Case 3",
					ExpectedResult = "Success",					
				},
				new DokumentlagerTestCase
				{
					Name = "Test Case 4",
					ExpectedResult = "Failure"					
				}
			}
		}
	};
}


#endif