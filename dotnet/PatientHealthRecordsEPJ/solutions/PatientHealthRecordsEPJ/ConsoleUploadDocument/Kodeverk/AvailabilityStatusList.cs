namespace ConsoleUploadDocument.Kodeverk; 
	
public class AvailabilityStatusList
{
	public static List<TextValue> AvailabilityStatuses = new List<TextValue>()
	{
		new TextValue() { Value = "urn:oasis:names:tc:ebxml-regrep:StatusType:Approved", Text = "Approved"  },
		new TextValue() { Value = "urn:oasis:names:tc:ebxml-regrep:StatusType:Deprecated", Text = "Deprecated"  }

	}; 
}
