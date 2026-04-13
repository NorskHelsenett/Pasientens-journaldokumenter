using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.Models;
using System.Collections.Concurrent;

namespace PatientHealthRecordsEPJ.DokumentlagerTest; 

public interface ITestRunStore
{
	TestRunInfo Create(PhrApiEnvironment apiEnvironment, string homeCommunityId, DokumentlagerTestCase test, SessionData sessessionData, int groupIndex, int testIndex, string patientId);	
	bool TryGet(string runId, out TestRunInfo? run);
	void Update(TestRunInfo run);
	void Clear();
	IEnumerable<TestRunInfo> GetAllRuns();
}

public sealed class InMemoryTestRunStore : ITestRunStore
{
	private readonly ConcurrentDictionary<string, TestRunInfo> _runs = new();

	public TestRunInfo Create(PhrApiEnvironment apiEnvironment, string homeCommunityId, DokumentlagerTestCase test, SessionData sessionData, int groupIndex, int testIndex, string patientId)
	{
		var run = new TestRunInfo
		{
			ApiEnvironment = apiEnvironment,
			HomeCommunityId = homeCommunityId,
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
	
	public IEnumerable<TestRunInfo> GetAllRuns()
	{
		return _runs.Values;
	}

	public void Clear()
	{
		_runs.Clear(); 
	}
}