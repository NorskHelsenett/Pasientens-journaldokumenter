using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public static class XslLocations
{
	public const string HTML = "http://www.w3.org/1999/xhtml";
	private static readonly IReadOnlyDictionary<string, string> xslMap;

	static XslLocations()
	{
		var xslModifiableMap = new Dictionary<string, string>
		{
			{ "http://www.kith.no/xmlstds/epikrise/2006-09-23", "xsl/epikrise/v1.1/Epikrise-v1_1-2html.xsl" },
			{ "http://www.kith.no/xmlstds/epikrise/2012-02-15", "xsl/epikrise/Epikrise2html.xsl" },
			{ "http://www.kith.no/xmlstds/henvisning/2005-07-08", "xsl/xsl/henvisning2html.xsl" },
			{ "http://www.kith.no/xmlstds/henvisning/2012-02-15", "xsl/xsl/henvisning2html.xsl" },
			{ "http://www.kith.no/xmlstds/rekvisisjon/2005-05-20", "xsl/xsl/rekvisisjon2html.xsl" },
			{ "http://www.kith.no/xmlstds/rekvisisjon/2008-12-01", "xsl/xsl/rekvisisjon2html.xsl" },
			{ "http://www.kith.no/xmlstds/rekvisisjon/2012-02-15", "xsl/xsl/rekvisisjon2html.xsl" },
			{ "http://www.kith.no/xmlstds/labsvar/2004-09-14", "xsl/xsl/svarrapport_v1_2_2004_2html.xsl" },
			{ "http://www.kith.no/xmlstds/labsvar/2005-03-14", "xsl/xsl/svarrapport_v1_2_2html.xsl" },
			{ "http://www.kith.no/xmlstds/labsvar/2008-12-01", "xsl/xsl/svarrapport2html.xsl" },
			{ "http://www.kith.no/xmlstds/labsvar/2012-02-15", "xsl/xsl/svarrapport2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/Pasientlogistikk/2008-02-20", "xsl/xsl/plo/plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/MeldingFravar/2008-02-20", "xsl/xsl/plo/plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/OrienteringOmTjenestetilbud/2008-02-20", "xsl/xsl/plo/plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/OverforingMedisinskeOpplysninger/2008-02-20", "xsl/xsl/plo/plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/Konsultasjon/2008-02-20 ", "xsl/xsl/plo/plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/Innleggelsesrapport/2008-02-20", "xsl/xsl/plo/plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/Innleggelsesrapport/2009-06-30", "xsl/xsl/plo/plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/TverrfagligEpikrise/2008-02-20", "xsl/xsl/plo/plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/msghead/2006-05-24", "xsl/dialogmelding/v1.1/Dialog-v1_1-2html.xsl" } 

			/*{ "http://www.kith.no/xmlstds/epikrise/2006-09-23", @"xsl\epikrise\v1.1\Epikrise-v1_1-2html.xsl" },
			{ "http://www.kith.no/xmlstds/epikrise/2012-02-15", @"xsl\epikrise\Epikrise2html.xsl" },
			{ "http://www.kith.no/xmlstds/henvisning/2005-07-08", @"xsl\xsl\henvisning2html.xsl" },
			{ "http://www.kith.no/xmlstds/henvisning/2012-02-15", @"xsl\xsl\henvisning2html.xsl" },
			{ "http://www.kith.no/xmlstds/rekvisisjon/2005-05-20", @"xsl\xsl\rekvisisjon2html.xsl" },
			{ "http://www.kith.no/xmlstds/rekvisisjon/2008-12-01", @"xsl\xsl\rekvisisjon2html.xsl" },
			{ "http://www.kith.no/xmlstds/rekvisisjon/2012-02-15", @"xsl/xsl\rekvisisjon2html.xsl" },
			{ "http://www.kith.no/xmlstds/labsvar/2004-09-14", @"xsl\xsl\svarrapport_v1_2_2004_2html.xsl" },
			{ "http://www.kith.no/xmlstds/labsvar/2005-03-14", @"xsl\xsl\svarrapport_v1_2_2html.xsl" },
			{ "http://www.kith.no/xmlstds/labsvar/2008-12-01", @"xsl\xsl\svarrapport2html.xsl" },
			{ "http://www.kith.no/xmlstds/labsvar/2012-02-15", @"xsl\xsl\svarrapport2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/Pasientlogistikk/2008-02-20", @"xsl\xsl\plo\plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/MeldingFravar/2008-02-20", @"xsl\xsl\plo\plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/OrienteringOmTjenestetilbud/2008-02-20", @"xsl\xsl\plo\plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/OverforingMedisinskeOpplysninger/2008-02-20", @"xsl\xsl\plo\plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/Konsultasjon/2008-02-20 ", @"xsl\xsl\ploplo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/Innleggelsesrapport/2008-02-20", @"xsl\xsl\plo\plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/Innleggelsesrapport/2009-06-30", @"xsl\xsl\plo\plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/po/TverrfagligEpikrise/2008-02-20", @"xsl\xsl\plo\plo2html.xsl" },
			{ "http://www.kith.no/xmlstds/msghead/2006-05-24", @"xsl\dialogmelding\v1.1\Dialog-v1_1-2html.xsl" }*/
		};

		xslMap = new ReadOnlyDictionary<string, string>(xslModifiableMap);
	}

	public static string GetXslLocation(string @namespace)
	{
		if (xslMap.TryGetValue(@namespace, out var location))
		{
			return location;
		}
		return null;
	}
}
