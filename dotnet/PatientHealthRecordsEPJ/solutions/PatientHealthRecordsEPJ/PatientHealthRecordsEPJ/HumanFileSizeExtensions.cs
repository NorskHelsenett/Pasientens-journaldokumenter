using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PatientHealthRecordsEPJ;

public static class HumanFileSizeExtensions
{
	public static string ToPrettySize(this long? size)
	{
		if (size.HasValue)
		{
			if (size > Constants.GB)
			{
				return ((size.Value / Constants.GB).ToString("#0.00")) + " GB";
			}
			else if (size.Value > Constants.MB)
			{
				return ((size.Value / Constants.MB).ToString("#0.00")) + " MB";
			}
			else if (size > Constants.KB)
			{
				return ((size.Value / Constants.KB).ToString("#0.00")) + " KB";
			}
			else
			{
				return size + " bytes";
			}
		}
		else
		{
			return "-";
		}
	}

	public static string ToPrettySize(this int? size)
	{
		return ((long?)size).ToPrettySize(); 
	}
}