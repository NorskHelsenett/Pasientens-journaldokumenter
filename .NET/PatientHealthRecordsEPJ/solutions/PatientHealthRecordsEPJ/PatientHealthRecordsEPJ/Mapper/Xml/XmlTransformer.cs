using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

public class XmlTransformer
{
	public static string TransformXml(string xmlString, string xslString)
	{
		// Load the XML document from the provided string
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xmlString);

		//XsltSettings xsltSettings = new XsltSettings
		//{
		//	EnableDocumentFunction = true,
		//	EnableScript = true
		//};

		var xsltSettings = new XsltSettings(true, true);

		// Load the XSLT stylesheet from the provided string
		XslCompiledTransform xslt = new XslCompiledTransform();

		var currentDirectory = System.IO.Directory.GetCurrentDirectory();

		var settings = new XmlReaderSettings
		{
			DtdProcessing = DtdProcessing.Parse,			
			XmlResolver = new LocalFileXmlResolver(currentDirectory)
		};

		using (StringReader sr = new StringReader(xslString))
		using (XmlReader xr = XmlReader.Create(sr, settings))
		{
			//xslt.Load(xr);
			xslt.Load(xr, xsltSettings, new LocalFileXmlResolver(currentDirectory));
		}

		// Perform the transformation
		using (StringWriter sw = new StringWriter())
		using (XmlWriter xw = XmlWriter.Create(sw, xslt.OutputSettings))
		{
			xslt.Transform(xmlDocument, xw);
			return sw.ToString();
		}
	}
}

/*
public class Program
{
	public static void Main(string[] args)
	{
		// Example XML and XSL strings
		string xmlString = @"
            <root>
                <greeting>Hello, World!</greeting>
            </root>";

		string xslString = @"
            <xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
                <xsl:template match='/'>
                    <html>
                        <body>
                            <h1><xsl:value-of select='/root/greeting'/></h1>
                        </body>
                    </html>
                </xsl:template>
            </xsl:stylesheet>";

		string htmlOutput = XmlTransformer.TransformXml(xmlString, xslString);

		// Print the transformed HTML
		Console.WriteLine(htmlOutput);
	}
}
*/