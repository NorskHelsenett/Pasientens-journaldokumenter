namespace NorskHelsenettUpload;

public class SimpleCoding
{
	public string? System { get; set; }
	public string? Code { get; set; }
	public string? Display { get; set; }

	public string ToValueString()
	{
		return $"{System}|{Code}|{Display}"; 
	}
}
