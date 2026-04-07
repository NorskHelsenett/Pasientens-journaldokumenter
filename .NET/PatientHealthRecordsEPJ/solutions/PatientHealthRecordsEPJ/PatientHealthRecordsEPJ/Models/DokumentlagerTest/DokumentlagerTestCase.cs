namespace PatientHealthRecordsEPJ.Models;

public enum DokumentlagerTestStatusEnum
{
	Stopped,
	Running,
	Paused,
	Success,
	Failed
};

public enum DokumentlagerTestCaseStatus
{
	NotRun,
	Queued,	
	Running,
	Success,
	Failed
};

public enum LookupDocumentReferenceStatus
{
	NotRun,
	//Queued,
	Running,
	Visible,
	VisibleWithObfuscation,
	NotVisible
	//OpenDocumentSuccess,
	//OpenDocumentFailed
	//Success,
	//Failed
};

public enum LookupDocumentStatus
{
	NotRun,
	//Queued,
	Running,	
	OpenDocumentSuccess,
	OpenDocumentFailed
	//Success,
	//Failed
};

public class DokumentlagerTestStatus
{
	public DokumentlagerTestStatusEnum Status { get; set; } = DokumentlagerTestStatusEnum.Stopped;	
}

public class DokumentlagerTestGroup
{
	public string? Name { get; set; }

	public List<DokumentlagerTestCase>? TestCases { get; set; } = null;
}

public class DokumentlagerTestCase
{
	public int Id { get; set; }
	public DokumentlagerTestCaseStatus Status { get; set; } = DokumentlagerTestCaseStatus.NotRun;
	public string? Name { get; set; }
	public string? ExpectedResult { get; set; }
	public string? Comment { get; set; }
	public string? Details { get; set; }

	public bool CreateRandomDocument { get; set; } = true;

	// Custom metadata when uploading
	public List<string> SecurityLabels { get; set; } = new List<string>();

	// Custom metadata when retrieving document reference or opening document, used for lookups and expected results
	public string? PurposeOfUse { get; set; }
	public string? GrunnlagId { get; set; } // nhn-access-basis. If null then SAMTYKKE is used. 
	public DocumentListScopeEnum DocumentListScope { get; set; } = DocumentListScopeEnum.Kjernejournalforskriften;  // DocumentListScopeEnum.HelsepersonellovenParagraf45 or DocumentListScopeEnum.Kjernejournalforskriften

	//public DokumentlagerTestCaseExpected? Expected { get; set; }
	public DokumentlagerTestCaseLookup? Lookup { get; set; } = new DokumentlagerTestCaseLookup();
	public DokumentlagerTestEvents? Events { get; set; }	// Simulate death or address protection
	public DokumentlagerTestCaseLookup? LookupAfterEvents { get; set; } // = new DokumentlagerTestCaseLookup();

	// PATCH document
	public bool IsTryPatchDocument { get; set; } = true;
	public DokumentlagerTestCaseStatus PatchDocumentStatus { get; set; } = DokumentlagerTestCaseStatus.NotRun;

	// DELETE document
	public bool IsTryDeleteDocument { get; set; } = true;
	public DokumentlagerTestCaseStatus DeleteDocumentStatus { get; set; } = DokumentlagerTestCaseStatus.NotRun;

	public DokumentlagerTestResult? UploadDocumentResult { get; set; }

	public DokumentlagerTestResult? Result { get; set; } = new DokumentlagerTestResult();    // Overall result for the test case, based on the lookups and expected results	
}

public class DokumentlagerTestEvents
{
	public bool IsTrySimulateDeath { get; set; }
	public bool IsTrySimulateAddressProtection { get; set; }

	public bool SimulateDeathExpectScheduledDocumentDeletion { get; set; } = true; // After 30 days
	public bool SimulateDeathIsSuccess { get; set; }

	public bool SimulateAddressProtectionExpectDocumentDeletion { get; set; } = true;	// Delete documents immediatly, for V NORN_ANG
	public bool SimulateAddressProtectionIsSuccess { get; set; }

	public DokumentlagerTestCaseStatus SimulateDeathStatus { get; set; } = DokumentlagerTestCaseStatus.NotRun;
	public DokumentlagerTestCaseStatus SimulateAddressProtectionStatus { get; set; } = DokumentlagerTestCaseStatus.NotRun;
}

//public class DokumentlagerTestCaseExpected
public class DokumentlagerTestCaseLookup
{
	//public bool LookupDocumentReference { get; set; }

	public Lookup? LookupDokumentlager { get; set; } = new Lookup();	// Lookup direct against NHN dokumentlager instance
	public Lookup? LookupAPI { get; set; } = new Lookup();
	public Lookup? LookupKJ { get; set; } = new Lookup();
	public Lookup? LookupHelsenorge { get; set; } = new Lookup();	
}

public class Lookup
{
	// Lookup document reference
	public LookupDocumentReferenceStatus RetrieveDocumentReferenceStatus { get; set; } = LookupDocumentReferenceStatus.NotRun;
	public bool IsTryRetrieveDocumentReference { get; set; } = true;	

	public bool RetrieveDocumentReferenceExpectVisible { get; set; } = true;
	public bool RetrieveDocumentReferenceIsSuccess { get; set; } // = true;

	public bool RetrieveDocumentReferenceExpectVisibleWithObfuscation { get; set; } = false;
	public bool RetrieveDocumentReferenceIsSuccessWithObfuscation { get; set; } = false;
	
	// Lookup document 
	public LookupDocumentStatus OpenDocumentStatus { get; set; } = LookupDocumentStatus.NotRun;
	public bool IsTryOpenDocument { get; set; } = true;

	public bool OpenDocumentExpectSuccess { get; set; } = true;
	public bool OpenDocumentIsSuccess { get; set; }

	// Lookup ATNA/Audit Event log
	public bool IsTryReadAuditEvents { get; set; } = true;

	//// PATCH document
	//public bool IsTryPatchDocument { get; set; } = true;

	//// DELETE document
	//public bool IsTryDeleteDocument { get; set; } = true;


	//public bool Success { get; set; }	// Overall success
	public string? ErrorMessage { get; set; }

	public bool Success // Overall success
	{
		get
		{
			// Determine overall success for API lookup based on the expectations and results for the different steps (retrieving document reference and opening document)

			var success = false; 
			
			if (IsTryRetrieveDocumentReference && IsTryOpenDocument)
			{
				success = RetrieveDocumentReferenceIsSuccess && OpenDocumentIsSuccess;
			}
			else if (IsTryRetrieveDocumentReference)
			{
				success = RetrieveDocumentReferenceIsSuccess;
			}
			else if (IsTryOpenDocument)
			{
				success = OpenDocumentIsSuccess;
			}

			return success;
		}
	}
}

public class DokumentlagerTestResult
{	
	public bool Success { get; set; }
	public string? Message { get; set; }
	public string? ErrorMessage { get; set; }
}

public sealed class TestRunInfo
{
	public string HomeCommunityId { get; set; }
	public string RunId { get; set; } = Guid.NewGuid().ToString("N");
	public int GroupIndex { get; set; }
	public int TestIndex { get; set; }
	public string PatientId { get; set; } = "";
	public SessionData SessionData { get; set; } = null!; 
	public DokumentlagerTestCaseStatus Status { get; set; }
	public DokumentlagerTestCase? Test { get; set; }
	public DokumentlagerTestResult? Result { get; set; }
	public string? Error { get; set; }
	public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
}