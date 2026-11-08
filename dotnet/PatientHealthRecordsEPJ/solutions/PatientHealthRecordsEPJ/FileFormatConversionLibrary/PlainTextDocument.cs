namespace FileFormatConversionLibrary;

using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using PdfSharpCore.Pdf;

public class PlainTextDocument
{
    public static byte[] GetRendered(string text)
    {
        // Create a MigraDocCore document
        Document document = CreateDocument(text);


        PdfDocumentRenderer renderer = new(true);

        renderer.Document = document;
        renderer.RenderDocument();

        MemoryStream PdfStream = new();
        renderer.PdfDocument.Save(PdfStream);

        return PdfStream.ToArray();
    }

    private static Document CreateDocument(string text)
    {
        // Create a new MigraDocCore document
        Document document = new();
        document.Info.Title = "file.txt";
        document.Info.Subject = "Converted TXT-file";
        document.Info.Author = "Norsk helsenett SF";

        DefineStyles(document);
        DefineContentSection(document);
        AddText(document, text);

        return document;
    }

    /// <summary>
    /// Defines page setup, headers, and footers.
    /// </summary>
    private static void DefineContentSection(Document document)
    {
        var section = document.AddSection();
        section.PageSetup.StartingNumber = 1;
    }

    /// <summary>
    /// Defines the styles used in the document.
    /// </summary>
    private static void DefineStyles(Document document)
    {
        Style style = document.Styles["Normal"];
        // Because all styles are derived from Normal, the next line changes the
        // font of the whole document. Or, more exactly, it changes the font of
        // all styles and paragraphs that do not redefine the font.
        style.Font.Name = "Courier New";


    }

    private static void AddText(Document document, string text)
    {
        //document.LastSection.AddParagraph();
        //var section = document.LastSection;
        var paragraph = document.LastSection.AddParagraph();
        paragraph.AddText(text);
        paragraph.AddLineBreak();
    }



}