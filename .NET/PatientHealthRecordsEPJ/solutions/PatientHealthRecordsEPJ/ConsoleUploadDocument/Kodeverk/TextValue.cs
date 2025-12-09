namespace ConsoleUploadDocument.Kodeverk;

public class TextValue
{
	public string Text { get; set; }
	public string Value { get; set; }

	public TextValue()
	{		
	}

	public TextValue(string value, string text)
	{		
		Value = value;
		Text = text;
	}
}
