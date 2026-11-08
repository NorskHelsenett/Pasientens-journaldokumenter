namespace ConsoleUploadDocument.Kodeverk;

public class CompositionFormatList
{
	public static List<TextValue> CompositionFormats = new List<TextValue>()
	{
		  new TextValue() { Value = "urn:ihe:iti:xds:2017:mimeTypeSufficient", Text = "mimeTypeSufficient" },
		  new TextValue() { Value = "urn:hl7-org:sdwg:ccda-nonXMLBody:2.1", Text = "ccda-nonXMLBody:2.1" },
		  new TextValue() { Value = "urn:hl7-org:sdwg:ccda-structuredBody:2.1", Text = "ccda-structuredBody:2.1" },
		  new TextValue() { Value = "urn:no:kith:xmlstds:dialog:2006-10-11", Text = "Dialog [KITH]" },
		  new TextValue() { Value = "urn:no:kith:xmlstds:epikrise:2006-09-23", Text = "Epikrise 1.1 [KITH]" },
		  new TextValue() { Value = "urn:no:kith:xmlstds:epikrise:2012-02-15", Text = "Epikrise 1.2 [KITH]" },
		  new TextValue() { Value = "urn:no:kith:xmlstds:labsvar:2012-02-15", Text = "Svarraport 1.4 [KITH]" },
		 
	}; 
}
