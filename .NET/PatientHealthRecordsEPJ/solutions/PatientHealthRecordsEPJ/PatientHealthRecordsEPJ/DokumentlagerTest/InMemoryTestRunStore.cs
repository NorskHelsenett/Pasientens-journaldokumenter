using PatientHealthRecordsEPJ.Models;
using System.Collections.Concurrent;

namespace PatientHealthRecordsEPJ.DokumentlagerTest; 

public interface ITestRunStore
{
	TestRunInfo Create(DokumentlagerTestCase test, SessionData sessessionData, int groupIndex, int testIndex, string patientId);
	bool TryGet(string runId, out TestRunInfo? run);
	void Update(TestRunInfo run);
}

public sealed class InMemoryTestRunStore : ITestRunStore
{
	private readonly ConcurrentDictionary<string, TestRunInfo> _runs = new();

	public TestRunInfo Create(DokumentlagerTestCase test, SessionData sessionData, int groupIndex, int testIndex, string patientId)
	{
		var run = new TestRunInfo
		{
			GroupIndex = groupIndex,
			TestIndex = testIndex,
			PatientId = patientId,
			SessionData = sessionData,
			Status = DokumentlagerTestCaseStatus.Queued,
			Test = test
		};

		_runs[run.RunId] = run;
		return run;
	}

	public bool TryGet(string runId, out TestRunInfo? run)
	{
		var ok = _runs.TryGetValue(runId, out var value);
		run = value;
		return ok;
	}

	public void Update(TestRunInfo run)
	{
		_runs[run.RunId] = run;
	}
}