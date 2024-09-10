using PatientHealthRecordsEPJ.Mapper.Xml;

public class XmlTemplateCache
{
	private readonly Dictionary<string, XmlTemplate> xmlMapperCache = new Dictionary<string, XmlTemplate>();

	public XmlTemplate GetTemplate(string xsl)
	{
		if (!xmlMapperCache.ContainsKey(xsl))
		{
			xmlMapperCache[xsl] = new XmlTemplate(xsl);
		}
		return xmlMapperCache[xsl];
	}
}

/*public class XmlTemplate
{
	public string Xsl { get; }

	public XmlTemplate(string xsl)
	{
		Xsl = xsl;
	}
}
*/