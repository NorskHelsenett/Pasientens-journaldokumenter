using System;
using System.Collections.Generic;
using System.Text;

namespace NorskHelsenettUpload;

public enum PersonalNumberType
{
	FNumber,
	DNumber,
	HNumber,    // Felles hjelpenummer
	Unknown
}

public class PersonalNumber
{
	public PersonalNumberType NumberType { get; set; }
	public string? System { get; set; }
	public string? SystemDisplayText { get; set; }

	public PersonalNumber(string? inputNin)
	{
		if (string.IsNullOrWhiteSpace(inputNin) || inputNin.Length != 11)
		{
			NumberType = PersonalNumberType.Unknown;
			return;
		}

		var day = int.Parse(inputNin.Substring(0, 2));
		var month = int.Parse(inputNin.Substring(2, 2));
		var year = inputNin.Substring(4, 2);
		var control = inputNin.Substring(6, 3);

		NumberType = (day, month) switch
		{
			// Synthetic test-data NINs
			var (d, m) when m - 65 is > 0 and <= 12 || m - 80 is > 0 and <= 12
				=> d - 40 is > 0 and <= 31
					? PersonalNumberType.DNumber
					: PersonalNumberType.FNumber,

			// Normal D-number (+40 on day)
			var (d, _) when d - 40 is > 0 and <= 31
				=> PersonalNumberType.DNumber,

			// Normal H-number (+40 on month)
			var (_, m) when m - 40 is > 0 and <= 12
				=> PersonalNumberType.HNumber,

			// Otherwise, regular F-number
			_ => PersonalNumberType.FNumber
		};

		switch (NumberType)
		{
			case PersonalNumberType.FNumber:
				System = Constants.Oid.Fnr;
				SystemDisplayText = "F-Number";
				break;
			case PersonalNumberType.DNumber:
				System = Constants.Oid.Dnr;
				SystemDisplayText = "D-Number";
				break;
			case PersonalNumberType.HNumber:
				System = Constants.Oid.Hnr;
				SystemDisplayText = "H-Number";
				break;
		}
	}
}
