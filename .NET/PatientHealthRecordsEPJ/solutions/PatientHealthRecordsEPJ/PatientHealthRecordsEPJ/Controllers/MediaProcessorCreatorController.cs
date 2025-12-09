using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;

namespace PatientHealthRecordsEPJ.Controllers; 

[Route("api/[controller]")]
[ApiController]
public class MediaProcessorCreatorController : ControllerBase
{       
    public MediaProcessorCreatorController()
    {        
    }

    /// <summary>
    /// Creates PDF-document from plain text file
    /// </summary>
    /// <returns>PDF-file</returns>
    [HttpPost("text2pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> PdfFromTxtAsBase64AsStringAsync()
    {
        string content = "";
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            content = await reader.ReadToEndAsync();
        }

        byte[] decodedBytes = Convert.FromBase64String(content);
        
        var pdf = FileFormatConversionLibrary.PlainTextDocument.GetRendered(Encoding.UTF8.GetString(decodedBytes));

        return File(pdf, "application/pdf; charset=utf-8", "migra.pdf");
    }

    /// <summary>
    /// Creates PDF-document from rich text file
    /// </summary>
    /// <returns>HTML-file</returns>
    [HttpPost("rtf2html")]
    [Produces("text/html")]
    public async Task<IActionResult> HtmlFromRtfAsBase64AsStringAsync()
    {
        string content = "";
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            content = await reader.ReadToEndAsync();
        }

        byte[] decodedBytes = Convert.FromBase64String(content);

        var html = FileFormatConversionLibrary.RichTextDocument.GetRendered(Encoding.UTF8.GetString(decodedBytes));

        return File(html, "text/html; charset=utf-8", "rtf.html");
    }

}
