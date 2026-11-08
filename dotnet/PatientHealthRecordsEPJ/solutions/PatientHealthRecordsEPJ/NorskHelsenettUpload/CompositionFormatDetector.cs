using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace NorskHelsenettUpload; 

public static class CompositionFormatDetector
{
/*
	PDF or any non-XML file → urn:ihe:iti:xds:2017:mimeTypeSufficient.
	CDA + nonXMLBody(PDF) → urn:hl7-org:sdwg:ccda-nonXMLBody:2.1.
	CDA + nonXMLBody(application/xml) → detect embedded KITH if possible; else ccda-nonXMLBody:2.1.
	CDA + structuredBody → urn:hl7-org:sdwg:ccda-structuredBody:2.1.
	KITH XML directly → map to the correct KITH URN based on the root namespace.
	Everything else XML → fall back to mimeTypeSufficient.
*/

	public static string? DetermineFormat(byte[] bytes, ref string mimeType, string? fileName = null)
	{
		if (bytes is null || bytes.Length == 0) return null;

		// 1) Fast non-XML checks (PDF magic number)
		if (IsPdf(bytes))
		{
			mimeType = "application/pdf";
			return "urn:ihe:iti:xds:2017:mimeTypeSufficient";
		}

		// 2) Is XML?
		if (!LooksLikeXml(bytes))
		{
			// Unknown non-XML -> fall back
			return "urn:ihe:iti:xds:2017:mimeTypeSufficient";
		}

		var doc = LoadXml(bytes);
		var root = doc.DocumentElement;
		if (root == null) return null;

		// 3) CDA?
		if (root.LocalName == "ClinicalDocument" && root.NamespaceURI.Contains("urn:hl7-org:v3"))
		{
			var nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("hl7", root.NamespaceURI);

			// Prefer language from CDA if you want:
			// var lang = root.SelectSingleNode("/hl7:ClinicalDocument/hl7:languageCode", nsmgr)
			//               ?.Attributes?["code"]?.Value;

			// (a) CDA with nonXMLBody?
			var textNode = root.SelectSingleNode("//hl7:component/hl7:nonXMLBody/hl7:text", nsmgr) as XmlElement;
			if (textNode != null)
			{
				var mediaType = textNode.GetAttribute("mediaType");
				var repr = textNode.GetAttribute("representation");

				// PDF in nonXMLBody (usually base64)
				if (mediaType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
				{
					//mimeType = "application/pdf";
					return "urn:hl7-org:sdwg:ccda-nonXMLBody:2.1";
				}

				// NonXMLBody carrying KITH XML
				if (mediaType.Equals("application/xml", StringComparison.OrdinalIgnoreCase))
				{
					// If you want to peek inside, decode when B64:
					string embeddedXml = repr.Equals("B64", StringComparison.OrdinalIgnoreCase)
						? Encoding.UTF8.GetString(Convert.FromBase64String(textNode.InnerText))
						: textNode.InnerText;

					// Try to detect KITH inside the CDA
					var kith = TryDetectKithFormatFromXmlString(embeddedXml);
					if (kith != null) return kith;

					// Otherwise treat as CDA with nonXMLBody
					return "urn:hl7-org:sdwg:ccda-nonXMLBody:2.1";
				}

				// Unknown nonXMLBody -> still CDA nonXMLBody
				return "urn:hl7-org:sdwg:ccda-nonXMLBody:2.1";
			}

			// (b) CDA with structuredBody?
			var structuredBody = root.SelectSingleNode("//hl7:component/hl7:structuredBody", nsmgr);
			if (structuredBody != null)
				return "urn:hl7-org:sdwg:ccda-structuredBody:2.1";

			// Fallback: CDA but no clear body -> CCDA structured as default
			return "urn:hl7-org:sdwg:ccda-structuredBody:2.1";
		}

		// 4) KITH direct (no CDA wrapper)
		var kithFormat = TryDetectKithFormatFromXmlDocument(doc);
		if (kithFormat != null) return kithFormat;

		// 5) Unknown XML -> if it’s XML but not CDA/KITH, use mimeTypeSufficient
		return "urn:ihe:iti:xds:2017:mimeTypeSufficient";
	}

	private static bool IsPdf(byte[] bytes)
		=> bytes.Length >= 5 &&
		   bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46 && bytes[4] == 0x2D; // %PDF-

	private static bool LooksLikeXml(byte[] bytes)
	{
		// Skip BOM/whitespace and look for '<'
		int i = 0;
		while (i < bytes.Length && char.IsWhiteSpace((char)bytes[i])) i++;
		// UTF-8 BOM
		if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) i = 3;
		return i < bytes.Length && bytes[i] == (byte)'<';
	}

	private static XmlDocument LoadXml(byte[] bytes)
	{
		var settings = new XmlReaderSettings
		{
			DtdProcessing = DtdProcessing.Prohibit,
			XmlResolver = null
		};
		using var ms = new MemoryStream(bytes, writable: false);
		using var xr = XmlReader.Create(ms, settings);
		var doc = new XmlDocument { XmlResolver = null };
		doc.Load(xr);
		return doc;
	}

	private static string? TryDetectKithFormatFromXmlDocument(XmlDocument doc)
	{
		var root = doc.DocumentElement;
		if (root == null) return null;

		var ns = root.NamespaceURI ?? string.Empty;

		// Map well-known KITH namespaces to your CompositionFormats
		if (ns.Equals("urn:no:kith:xmlstds:dialog:2006-10-11", StringComparison.OrdinalIgnoreCase))
			return "urn:no:kith:xmlstds:dialog:2006-10-11";

		if (ns.Equals("urn:no:kith:xmlstds:epikrise:2006-09-23", StringComparison.OrdinalIgnoreCase))
			return "urn:no:kith:xmlstds:epikrise:2006-09-23";

		if (ns.Equals("urn:no:kith:xmlstds:epikrise:2012-02-15", StringComparison.OrdinalIgnoreCase))
			return "urn:no:kith:xmlstds:epikrise:2012-02-15";

		if (ns.Equals("urn:no:kith:xmlstds:labsvar:2012-02-15", StringComparison.OrdinalIgnoreCase))
			return "urn:no:kith:xmlstds:labsvar:2012-02-15";

		// Some KITH docs use a root element without namespace but with known xsi:schemaLocation
		// You could add extra heuristics here if needed.

		return null;
	}

	private static string? TryDetectKithFormatFromXmlString(string xml)
	{
		try
		{
			var bytes = Encoding.UTF8.GetBytes(xml);
			var doc = LoadXml(bytes);
			return TryDetectKithFormatFromXmlDocument(doc);
		}
		catch
		{
			return null;
		}
	}
}
