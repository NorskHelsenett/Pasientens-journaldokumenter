using System.Data.SqlTypes;
using System.Xml;

namespace PatientHealthRecordsEPJ.Mapper.Xml;

public class XmlTemplate
{
	//private final Templates template;
	//private static final Logger log = LoggerFactory.getLogger(XmlTemplate.class);

	XmlTransformer xmlTransformer;

	private string xslFilePath; 

	public XmlTemplate(string xsl)
	{
		xslFilePath = xsl;

		xmlTransformer = new XmlTransformer();

		/*TransformerFactory transformerFactory = TransformerFactory.newInstance();
		transformerFactory.setAttribute(XMLConstants.ACCESS_EXTERNAL_DTD, "");
		// Restricts to only local files
		transformerFactory.setAttribute(XMLConstants.ACCESS_EXTERNAL_STYLESHEET, "file, zip");

		// JDK-en har noen standardgrenser for XML-prosessering som disse flotte
		// KITH-skjemaene overgår (LEKER DA IKKE XML HELLER), så vi må øke de.
		// Verdiene under her er nøyaktig det vi trenger (p.t.).
		try
		{
			transformerFactory.setAttribute("jdk.xml.xpathExprGrpLimit", "16");
			transformerFactory.setAttribute("jdk.xml.xpathExprOpLimit", "101");
			transformerFactory.setAttribute("jdk.xml.xpathTotalOpLimit", "150000");
		}
		catch (IllegalArgumentException e)
		{
			// Disse propertyene ble innført i 8u331, og eldre jdk-er kjenner
			// de ikke igjen, og blir sint og fortvila om man prøver å sette de.
			// Grensene kom sammen med propertyene, så vi bare ignorerer i det stille.
		}

		InputStream xslStream = XmlTemplate.class.getResourceAsStream(xsl);
		URL xslUrl = XmlTemplate.class.getResource(xsl);
		StreamSource styleSource = new StreamSource(xslStream, xslUrl.toString());

			try {
				template = transformerFactory.newTemplates(styleSource);
			} catch (TransformerConfigurationException e) {
				throw new ApplikasjonException("KJS-000304", e);
	} */

	}

	// Transformerer XML til HTML, vasker HTML, og konverterer bilde-vedlegg til PNG
	public string Transform(string reformatted)
	{
		string reformattedWithImageMimeType = InsertImageMimeType(reformatted);

		var xslString = System.IO.File.ReadAllText(xslFilePath);

		var folder = System.IO.Path.GetDirectoryName(xslFilePath);

		System.IO.Directory.SetCurrentDirectory(folder); 

		var html = XmlTransformer.TransformXml(reformattedWithImageMimeType, xslString);

		return html; 

		/*try (OutputStream outputStream = new ByteArrayOutputStream();
		InputStream inputStream = new ByteArrayInputStream(reformattedWithImageMimeType.getBytes(StandardCharsets.UTF_8))) {

			Transformer transformer = template.newTransformer();
			SAXSource saxSource = new SAXSource(new InputSource(inputStream));
			transformer.transform(saxSource, new StreamResult(outputStream));

			String outputStreamString = outputStream.toString();
			String sanitizedHtml = HtmlSanitizer.sanitizeHtml(outputStreamString);
			String transformTiffImages = HtmlSanitizer.transformTiffImages(sanitizedHtml);

			return Base64Util.encodeToString(transformTiffImages);

		} catch (TransformerException | IOException e) {
			throw new ApplikasjonException("KJS-000305", e);
		}
		} */

		//return reformattedWithImageMimeType;
	}

	// Legger til MimeType for bildevedlegg slik at img-taggen kommer med i HTML'en og vises i frontend
	public string InsertImageMimeType(string xml)
	{
		var doc = new XmlDocument();
		doc.LoadXml(xml);

		var namespaceManager = new XmlNamespaceManager(doc.NameTable);
		namespaceManager.AddNamespace("ns", doc.DocumentElement.NamespaceURI);
		//namespaceManager.AddNamespace("", doc.DocumentElement.NamespaceURI);

		//var idNodes = doc.SelectNodes("//Id", namespaceManager);
		var idNodes = doc.SelectNodes("//ns:Id", namespaceManager);
		//var idNodes = doc.SelectNodes("//Id");
		if (idNodes == null || idNodes.Count == 0)
		{
			return xml;
		}

		foreach (XmlNode idNode in idNodes)
		{
			string fileId = idNode.InnerText;

			if (fileId.ToLower().EndsWith(".tif") || fileId.ToLower().EndsWith(".tiff") ||
				fileId.ToLower().EndsWith(".jpg") || fileId.ToLower().EndsWith(".jpeg"))
			{
				var contentElement = idNode.NextSibling?.NextSibling;

				if (contentElement != null && contentElement.Name == "Content")
				{
					// Create and add MimeType element just before the Content element
					var mimeTypeElement = doc.CreateElement("MimeType");

					if (fileId.ToLower().EndsWith(".tif") || fileId.ToLower().EndsWith(".tiff"))
					{
						mimeTypeElement.InnerText = "image/tiff";
					}
					else
					{
						mimeTypeElement.InnerText = "image/jpeg";
					}
					contentElement.ParentNode.InsertBefore(mimeTypeElement, contentElement);
				}
			}
		}

		return doc.OuterXml;
	}


	// Legger til MimeType for bildevedlegg slik at img-taggen kommer med i HTML'en og vises i frontend
	/*public static string InsertImageMimeType(string xml)
{
Document doc = Jsoup.parse(xml, "", Parser.xmlParser());
final Elements ids = doc.select("Id");
if (ids.isEmpty())
{
	return xml;
}

for (Element id : ids) {
String fileId = id.text();

if (fileId.toLowerCase().endsWith(".tif") || fileId.toLowerCase().endsWith(".tiff") ||
fileId.toLowerCase().endsWith(".jpg") || fileId.toLowerCase().endsWith(".jpeg"))
{
Element contentElement = id.nextElementSibling().nextElementSibling();

if (contentElement != null && contentElement.tagName().equals("Content"))
{
// Opprett og legg til MimeType-elementet rett før Content-elementet
Element mimeTypeElement = new Element(Tag.valueOf("MimeType"), doc.baseUri());

if (fileId.toLowerCase().endsWith(".tif") || fileId.toLowerCase().endsWith(".tiff"))
{
	mimeTypeElement.text("image/tiff");
}
else
{
	mimeTypeElement.text("image/jpeg");
}
contentElement.before(mimeTypeElement);
}
}
  }

  return doc.toString();
}


}*/
}