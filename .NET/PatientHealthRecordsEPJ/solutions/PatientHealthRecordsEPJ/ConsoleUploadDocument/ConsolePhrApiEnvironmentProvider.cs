namespace PatientHealthRecords.ApiService;

public class ConsolePhrApiEnvironmentProvider : IPhrApiEnvironmentProvider
{	
	private readonly PhrApiOptions _options;	

	public ConsolePhrApiEnvironmentProvider(PhrApiOptions options)
	{		
		_options = options;
	}

	public void SetCurrentEnvironment(string environmentId)
	{
		//_httpContextAccessor.HttpContext!.Session.SetString(SessionKey, environmentId);
	}

	public PhrApiEnvironment GetCurrentEnvironment()
	{
		var envId = _options.ActiveEnvironment; // default fallback

		var env = _options.Environments.FirstOrDefault(x => x.Environment == envId);
		if (env == null)
			throw new InvalidOperationException($"Environment '{envId}' not found in configuration.");

		return env;
	}
}
