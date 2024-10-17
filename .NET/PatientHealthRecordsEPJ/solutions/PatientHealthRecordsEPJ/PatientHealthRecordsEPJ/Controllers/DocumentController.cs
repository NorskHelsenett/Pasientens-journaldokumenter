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
				new KeyValuePair<string, string>("hit-patient-pid", patient)
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
								// As an example we just read the base64 encoded PDF string

								var namespaceManager = new XmlNamespaceManager(doc.NameTable);
								namespaceManager.AddNamespace("hl7", doc.DocumentElement!.NamespaceURI);

								var textNode = doc.DocumentElement!.SelectSingleNode("//hl7:component/hl7:nonXMLBody/hl7:text", namespaceManager);

								if (textNode is not null)
								{
									if (textNode.Attributes["mediaType"]?.Value == "application/pdf" && textNode.Attributes["representation"]?.Value == "B64")
									{
										model.PdfBase64String = textNode.InnerText;
										model.ContentType = "application/pdf";

										textNode.InnerText = "...removed for display...";

										model.ClinicalDocumentXml = PrettyPrintXml(doc.InnerXml);
										model.IsShowClinicalDocumentXml = true;

										model.IsSuccess = true;
									}
								}
							}
							else
							{

								var ns = doc.DocumentElement!.NamespaceURI;

								var xslFilename = Path.Combine(_webHostEnvironment.ContentRootPath, "Resources", XslLocations.GetXslLocation(ns));

								XmlTemplate xmlTemplate = new XmlTemplate(xslFilename);

								var html = xmlTemplate.Transform(xml);

								model.Html = html; // Transfomert html vil bli lagret i en IFRAME på nettsiden

								model.ContentType = "application/xml";

								model.IsSuccess = true;
							}

							break;

						case "application/pdf":
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
