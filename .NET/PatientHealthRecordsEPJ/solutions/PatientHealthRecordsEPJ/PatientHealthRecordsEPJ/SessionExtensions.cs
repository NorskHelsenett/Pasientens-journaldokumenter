using System.Diagnostics;
using System.Text.Json; 

public static class SessionExtensions
{
	public static SessionData SessionData(this ISession session)
	{
		var serialized = session.GetString(Constants.SessionDataKey);

		if (!string.IsNullOrEmpty(serialized))
		{
			var sessionLoginResult = JsonSerializer.Deserialize<SessionData>(serialized);

			return sessionLoginResult!;
		}

		return new SessionData(); // .IsAuthenticated is false as default 
	}

	public static void SaveToSession(this SessionData sessionData, ISession session)
	{
		var serializedLoginResult = JsonSerializer.Serialize(sessionData);

		session.SetString(Constants.SessionDataKey, serializedLoginResult);		
	}
}