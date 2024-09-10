using System.Diagnostics;
using System.Text.Json; 

public static class SessionExtensions
{
	public static SessionLoginResult SessionLoginResult(this ISession session)
	{
		var serialized = session.GetString(Constants.SessionLoginResultKey);

		if (!string.IsNullOrEmpty(serialized))
		{
			var sessionLoginResult = JsonSerializer.Deserialize<SessionLoginResult>(serialized);

			return sessionLoginResult!;
		}

		return new SessionLoginResult(); // .IsAuthenticated is false as default 
	}

	public static void SaveToSession(this SessionLoginResult sessionLoginResult, ISession session)
	{
		var serializedLoginResult = JsonSerializer.Serialize(sessionLoginResult);

		session.SetString(Constants.SessionLoginResultKey, serializedLoginResult);		
	}
}