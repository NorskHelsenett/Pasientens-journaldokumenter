namespace PatientHealthRecordsEPJ; 

public class SessionActivityMiddleware
{
	private readonly RequestDelegate _next;

	public SessionActivityMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task Invoke(HttpContext context)
	{
		var session = context.Session;
		if (!session.Keys.Contains("LastAccess"))
		{
			session.SetString("LastAccess", DateTime.UtcNow.ToString("o")); // ISO format
		}
		else
		{
			session.SetString("LastAccess", DateTime.UtcNow.ToString("o"));
		}

		await _next(context);
	}
}
