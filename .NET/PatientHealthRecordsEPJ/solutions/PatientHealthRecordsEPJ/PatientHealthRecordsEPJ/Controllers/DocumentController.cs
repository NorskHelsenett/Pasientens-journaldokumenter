using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PatientHealthRecordsEPJ.Mapper.Xml;
using PatientHealthRecordsEPJ.Models;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PatientHealthRecordsEPJ.Controllers
{
	public class DocumentController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private ISession session => _httpContextAccessor.HttpContext!.Session;
		private readonly IWebHostEnvironment _webHostEnvironment;

		SessionLoginResult sessionLoginResult;

		public DocumentController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache, IWebHostEnvironment webHostEnvironment, ApiUrlSettings apiUrlSettings)
		{
			_logger = logger;
			_httpContextAccessor = httpContextAccessor;
			_webHostEnvironment = webHostEnvironment;
		}

		public async Task<IActionResult> Index(string url)
		{
			sessionLoginResult = session.SessionLoginResult();

			if (!sessionLoginResult.IsAuthenticated)
				return Redirect("/");

			var model = await GetDocument(sessionLoginResult.PatientId!, url);

			return View(model);
		}

		private async Task<GetDocumentViewModel> GetDocument(
			string patient,
			string url
			)
		{
			GetDocumentViewModel model = new GetDocumentViewModel();

			// We send the patient Id in the header as X-Patient-Id to avoid having the patient Id in the URL.             
			var headerKeyValues = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>("nhn-patient-nin", patient),
				new KeyValuePair<string, string>("nhn-source-system", "PJD Test EPJ 1.0"),
				new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString())
			};

			var apiResult = await ApiCaller.CallApi("GET", url, sessionLoginResult.AccessToken!, document: true, headerKeyValues: headerKeyValues);

			if (apiResult.IsSuccess)
			{
				try
				{
					switch (apiResult.ContentType)
					{
						case "application/xml":

							var xml = Encoding.UTF8.GetString(apiResult.Data!);

							XmlDocument doc = new XmlDocument();

							doc.LoadXml(xml);

							if (doc.DocumentElement!.Name == "ClinicalDocument")
							{
								// TODO: in a real application you could properly deserialize the clinical document. 
								// As an example we just read the base64 encoded PDF string, or the KITH xml string

								var namespaceManager = new XmlNamespaceManager(doc.NameTable);
								namespaceManager.AddNamespace("hl7", doc.DocumentElement!.NamespaceURI);

								var textNode = doc.DocumentElement!.SelectSingleNode("//hl7:component/hl7:nonXMLBody/hl7:text", namespaceManager);

								if (textNode is not null)
								{
									var mediaType = textNode.Attributes["mediaType"]?.Value;
									var representation = textNode.Attributes["representation"]?.Value;

									if (mediaType == "application/pdf" && representation == "B64")
									{
										model.PdfBase64String = textNode.InnerText;
										model.ContentType = "application/pdf";

										// Browsers have trouble displaying large PDF files properly when using base64 in the 'src' so create another solution with a
										// "DownloadPdf" action method and point the IFRAME src to that instead? 
										//var pdfBytes = Convert.FromBase64String(model.PdfBase64String); // With this I have verified that PDF files are in the correct format even though some can't currently be shown in the browser 

										textNode.InnerText = "...removed for display...";

										model.ClinicalDocumentXml = PrettyPrintXml(doc.InnerXml);
										model.IsShowClinicalDocumentXml = true;

										model.IsSuccess = true;

										return model;
									}
									else if (mediaType == "application/xml")
									{
										// Extract KITH xml from ClinicalDocument

										xml = string.Empty;

										xml = representation == "B64" ? Encoding.UTF8.GetString(Convert.FromBase64String(textNode.InnerText)) : textNode.InnerText;

										doc.LoadXml(xml);

										// Fall through to displaying KITH xml below
									}
								}
							}

							// The following code is run whether the document is an XML (kith) directly, or if it's an XML extracted from ClinicalDocument

							var ns = doc.DocumentElement!.NamespaceURI;

							var xslFilename = Path.Combine(_webHostEnvironment.ContentRootPath, "Resources", XslLocations.GetXslLocation(ns));

							XmlTemplate xmlTemplate = new XmlTemplate(xslFilename);

							var html = xmlTemplate.Transform(xml);

							model.Html = html; // Transfomert html vil bli lagret i en IFRAME på nettsiden

							model.ContentType = "application/xml";

							model.IsSuccess = true;


							break;

						case "application/pdf": // PDF not from within a ClinicalDocument
							model.PdfBase64String = Convert.ToBase64String(apiResult.Data!);
							model.ContentType = "application/pdf";
							model.IsSuccess = true;
							break;

						default:
							break;
					}

					return model;
				}
				catch (Hl7.Fhir.Serialization.DeserializationFailedException e)
				{
					foreach (var ex in e.Exceptions)
					{
						Debug.WriteLine(ex.Message);
					}

					model.IsSuccess = false;
					model.ErrorMessage = "Could not retreive document";
					return model;
				}
			}
			else
			{
				model.IsSuccess = false;
				model.ErrorMessage = "Could not retreive document";
				return model;
			}
		}

		public static string PrettyPrintXml(string xml)
		{
			// Load the XML string into an XDocument
			XDocument xDocument = XDocument.Parse(xml);

			// Use an XmlWriter with settings to format the XML
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "  ", // You can customize the indentation here
				NewLineChars = Environment.NewLine,
				NewLineHandling = NewLineHandling.Replace
			};

			using (var stringWriter = new System.IO.StringWriter())
			using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
			{
				xDocument.WriteTo(xmlWriter);
				xmlWriter.Flush();
				return stringWriter.GetStringBuilder().ToString();
			}
		}
	}
}
