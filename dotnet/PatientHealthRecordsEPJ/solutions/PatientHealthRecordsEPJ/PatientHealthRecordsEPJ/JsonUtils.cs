using System;
using System.Text.Json;

namespace PatientHealthRecordsEPJ;

public static class JsonUtils
{
	public static string Prettify(string? json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return string.Empty; 
		}		

		try
		{
			using var doc = JsonDocument.Parse(
				json,
				new JsonDocumentOptions
				{
					AllowTrailingCommas = true,
					CommentHandling = JsonCommentHandling.Skip
				});

			var opts = new JsonSerializerOptions { WriteIndented = true };
			return JsonSerializer.Serialize(doc.RootElement, opts);
		}
		catch (JsonException)
		{
			// If it's not valid JSON, just return the original text (or rethrow/log).
			return json;
		}
	}

	public static string PrettifyJson(this string? json)
	{
		return JsonUtils.Prettify(json);	
	}
}
