using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientHealthRecords.ApiService;

public interface IPhrApiEnvironmentProvider
{
	PhrApiEnvironment GetCurrentEnvironment();
	void SetCurrentEnvironment(string environmentId);
}

public class PhrApiEnvironmentProvider : IPhrApiEnvironmentProvider
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly PhrApiOptions _options;

	private const string SessionKey = "SelectedEnvironment";

	public PhrApiEnvironmentProvider(IHttpContextAccessor httpContextAccessor, PhrApiOptions options)
	{
		_httpContextAccessor = httpContextAccessor;
		_options = options;
	}

	public void SetCurrentEnvironment(string environmentId)
	{
		_httpContextAccessor.HttpContext!.Session.SetString(SessionKey, environmentId);
	}

	public PhrApiEnvironment GetCurrentEnvironment()
	{
		var envId = _httpContextAccessor.HttpContext!.Session.GetString(SessionKey)
					?? _options.ActiveEnvironment; // default fallback

		var env = _options.Environments.FirstOrDefault(x => x.Environment == envId);
		if (env == null)
			throw new InvalidOperationException($"Environment '{envId}' not found in configuration.");

		return env;
	}
}
