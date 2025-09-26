namespace FileFormatConversionLibrary;

using RtfPipe;
using System.Text;

public class RichTextDocument
{
    public static byte[] GetRendered(string text)
    {

        var html = CreateHtmlFromRtf(text);

        return Encoding.UTF8.GetBytes(html);
    }

    public static string CreateHtmlFromRtf(string text)
    {
        byte[] docRtf = Encoding.UTF8.GetBytes(text!);

        //var doc = new RtfPipe.Document();

        using TextReader tR = new StreamReader(new MemoryStream(docRtf));

        var rtfSource = new RtfSource(tR);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        return Rtf.ToHtml(rtfSource);
    }

}

