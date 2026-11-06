using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PatientHealthRecordsEPJ.ImageConversion;
using PatientHealthRecordsEPJ.Mapper.Xml;
using PatientHealthRecordsEPJ.Models;
using PatientHealthRecordsEPJ.Services;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace PatientHealthRecordsEPJ.Controllers; 

public class DocumentController : BaseController
{
	private readonly ILogger<HomeController> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;	
	private ISession session => _httpContextAccessor.HttpContext!.Session;
	private readonly IWebHostEnvironment _webHostEnvironment;
	private readonly IOidcClientService _oidcClientService;

	SessionData sessionLoginResult;

	public DocumentController(
		IOptions<SessionOptions> sessionOptions, 
		ILogger<HomeController> logger, 
		IHttpContextAccessor httpContextAccessor, 
		IMemoryCache memoryCache, 
		IWebHostEnvironment webHostEnvironment,		
		IOidcClientService oidcClientService,
		IUserTokenService userTokenService
		) : base(sessionOptions, userTokenService)
	{
		_logger = logger;
		_httpContextAccessor = httpContextAccessor;
		_webHostEnvironment = webHostEnvironment;
		_oidcClientService = oidcClientService;
	}

	private string GetRedirectUrl()
	{
#if DEBUG
		var redirectUrl = Request.Scheme + "://" + Request.Host.Value + "/callback";
#else
		var redirectUrl = HomeController.TestRedirectUrl; 
#endif
		return redirectUrl;
	}

	public async Task<IActionResult> Index(string url)
	{
		var loginResult = await _userTokenService.GetValidAccessTokensAsync(HttpContext);

		if (loginResult.PjdAccessToken == null)
		{
			return Redirect("/");
		}		

		sessionLoginResult = session.SessionData();

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
		Uri uri = new Uri(url);
		var query = QueryHelpers.ParseQuery(uri.Query);
		var documentUniqueId = query["documentUniqueId"];
		var tempDocs = Path.Combine(Path.GetTempPath(), "documents");	// In Program.cs there's a UseStaticFiles which serves from this path
		var patientFolder = Path.Combine(tempDocs, patient);
		var patientRelativePath = $"/documents/{patient}";		
		Directory.CreateDirectory(patientFolder);	 // For storing documents which will be displayed

		GetDocumentViewModel model = new GetDocumentViewModel();

		var headerKeyValues = new List<KeyValuePair<string, string>>();

		if (sessionLoginResult.DocumentListScopeId == (int)DocumentListScopeEnum.Kjernejournalforskriften)
		{
			// Only send access basis if using scope 'nhn:phr/mhd/read-documentreferences/kjernejournalforskriften', and only if FORHOYET_SAMTYKKE or FORHOYET_AKUTT

			if (sessionLoginResult.GrunnlagId?.Contains("forhoyet") == true)
			{
				headerKeyValues.Add(new KeyValuePair<string, string>("nhn-access-basis", sessionLoginResult.GrunnlagId ?? string.Empty));
			}
		}

		// We send the patient Id in the header as nhn-patient-nin to avoid having the patient Id in the URL.             

		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-patient-nin", patient));
		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-source-system", patient));
		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString())); 
		
		var apiResult = await ApiCaller.CallApi(HttpMethod.Get, url, sessionLoginResult.PjdAccessToken!, document: true, headerKeyValues: headerKeyValues);

		if (apiResult.IsSuccess)
		{
			try
			{
				if (apiResult.ContentType != "application/xml")
				{
					var extension = GetExtension(apiResult.ContentType!);

					var filename = documentUniqueId + extension;

					var filePath = Path.Combine(patientFolder, filename);

					System.IO.File.WriteAllBytes(filePath, apiResult.Data!);

					model.DocumentUrl = $"{patientRelativePath}/{filename}";
				}

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

							var clinicalDocumentFileName = $"{documentUniqueId!}_ClinicalDocument.xml";
							var clinicalDocumentfilePath = Path.Combine(patientFolder, clinicalDocumentFileName); 
							System.IO.File.WriteAllBytes(clinicalDocumentfilePath, apiResult.Data!);

							model.ClinicalDocumentUrl = $"{patientRelativePath}/{clinicalDocumentFileName}";

							var namespaceManager = new XmlNamespaceManager(doc.NameTable);
							namespaceManager.AddNamespace("hl7", doc.DocumentElement!.NamespaceURI);

							var textNode = doc.DocumentElement!.SelectSingleNode("//hl7:component/hl7:nonXMLBody/hl7:text", namespaceManager);

							var removeTextNodeForDisplay = false; 

							if (textNode is not null)
							{
								var mediaType = textNode.Attributes["mediaType"]?.Value;
								var representation = textNode.Attributes["representation"]?.Value;

								var extension = GetExtension(mediaType!);

								var filename = documentUniqueId + extension; 

								var filePath = Path.Combine(patientFolder, filename);

								var bytes = representation == "B64" ? Convert.FromBase64String(textNode.InnerText) : Encoding.UTF8.GetBytes(textNode.InnerText);

								System.IO.File.WriteAllBytes(filePath, bytes);

								model.DocumentUrl = $"{patientRelativePath}/{filename}";

								if (mediaType == "application/pdf" && representation == "B64")
								{
									removeTextNodeForDisplay = true;
																																	
									model.ContentType = "application/pdf";
																										
									model.IsSuccess = true;
								}
								else if ((mediaType == "application/rtf" || mediaType == "text/rtf") && representation == "B64")
								{ 
									removeTextNodeForDisplay = true;

									model.Html = await GetHtmlFromRtfAsync(textNode.InnerText); 
									
									model.ContentType = "application/rtf";

									model.IsConverted = true;
									model.ConvertedFromType = "RTF";
									model.ConvertedToType = "HTML";

									model.IsSuccess = true;									
								}
								else if (mediaType == "image/tiff" && representation == "B64")
								{
									// Browsers can't display TIFF, so convert it to PNG

									removeTextNodeForDisplay = true;
									
									string pngBase64 = ImageBase64Converter.TiffBase64ToPngBase64(
										textNode.InnerText,
										pageIndex: 0,
										pngCompressionLevel: SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.Level9 // smallest size, slowest
									);

									//File.WriteAllBytes("output.png", Convert.FromBase64String(pngBase64));
									
									model.ImageBase64String = pngBase64;
									//model.ContentType = "image/tiff";
									model.ContentType = "image/png";

									model.IsSuccess = true;
								}
								else if ((mediaType == "image/jpeg" || mediaType == "image/jpg") && representation == "B64")
								{
									removeTextNodeForDisplay = true;

									model.ImageBase64String = textNode.InnerText;

									model.ContentType = "image/jpeg";

									model.IsSuccess = true;
								}
								else if (mediaType == "image/png" && representation == "B64")
								{
									removeTextNodeForDisplay = true;

									model.ImageBase64String = textNode.InnerText;

									model.ContentType = "image/png";

									model.IsSuccess = true;
								}
								else if (mediaType == "image/gif" && representation == "B64")
								{
									removeTextNodeForDisplay = true;

									model.ImageBase64String = textNode.InnerText;

									model.ContentType = "image/gif";

									model.IsSuccess = true;
								}
								else if (mediaType == "application/xml")
								{
									removeTextNodeForDisplay = true;

									// Extract KITH xml from ClinicalDocument

									xml = string.Empty;

									xml = representation == "B64" ? Encoding.UTF8.GetString(Convert.FromBase64String(textNode.InnerText)) : textNode.InnerText;

									doc.LoadXml(xml);

									// Fall through to displaying KITH xml below
								}
								else if (mediaType == "text/plain")
								{
									var text = string.Empty;

									text = representation == "B64" ? Encoding.UTF8.GetString(Convert.FromBase64String(textNode.InnerText)) : textNode.InnerText;

									model.Html = text;

									model.ContentType = "text/plain";

									model.IsSuccess = true;																	
								}
								else if (mediaType == "application/fhir+json" || mediaType == "application/json")
								{
									var text = string.Empty;

									text = representation == "B64" ? Encoding.UTF8.GetString(Convert.FromBase64String(textNode.InnerText)) : textNode.InnerText;

									model.Html = text;

									model.ContentType = mediaType;

									model.IsSuccess = true;
								}
								else if (extension == ".bin") // Unknown media type, but we can still try to display it as text if it's base64 encoded and not too large
								{
									var text = string.Empty;

									text = representation == "B64" ? Encoding.UTF8.GetString(Convert.FromBase64String(textNode.InnerText)) : textNode.InnerText;

									model.Html = text;

									model.ContentType = mediaType;
									model.IsUnknownContentType = true; 

									model.IsSuccess = true;
								}

								if (removeTextNodeForDisplay)
								{
									textNode.InnerText = "...removed for display...";
								}

								model.ClinicalDocumentXml = PrettyPrintXml(doc.InnerXml);
								model.IsShowClinicalDocumentXml = true;

								if (model.IsSuccess)
								{
									return model; 
								}

							}
						}
						
						// The following code is run whether the document is an XML (kith) directly, or if it's an XML extracted from ClinicalDocument

						var ns = doc.DocumentElement!.NamespaceURI;

						var xslFilename = Path.Combine(_webHostEnvironment.ContentRootPath, "Resources", XslLocations.GetXslLocation(ns));

						XmlTemplate xmlTemplate = new XmlTemplate(xslFilename);

						var html = xmlTemplate.Transform(xml);

						model.Html = html; // Transfomert html vil bli lagret i en IFRAME p� nettsiden

						model.ContentType = "application/xml";

						model.IsSuccess = true;
						

						break;

					case "application/pdf": // PDF not from within a ClinicalDocument	
						{
							var filePath = Path.Combine(patientFolder, documentUniqueId!) + ".pdf";
							System.IO.File.WriteAllBytes(filePath, apiResult.Data!);

							model.DocumentUrl = $"/documents/{patient}/{documentUniqueId}.pdf";
							model.ContentType = "application/pdf";

							model.IsSuccess = true;
						}
						break;

					case "image/tif": // TIF not from within a ClinicalDocument	
					case "image/tiff": // TIFF not from within a ClinicalDocument	
						{							
							var tifBase64 = Convert.ToBase64String(apiResult.Data!);

							string pngBase64 = ImageBase64Converter.TiffBase64ToPngBase64(
								tifBase64,
								pageIndex: 0,
								pngCompressionLevel: SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.Level9 // smallest size, slowest
							);

							//File.WriteAllBytes("output.png", Convert.FromBase64String(pngBase64));

							model.ImageBase64String = pngBase64;
							//model.ContentType = "image/tiff";
							model.ContentType = "image/png";
							model.IsConverted = true;
							model.ConvertedFromType = "TIFF";
							model.ConvertedToType = "PNG"; 
							model.IsSuccess = true;
							break; 
						}

					case "image/png": // PNG not from within a ClinicalDocument	
					case "image/jpg": // JPG not from within a ClinicalDocument	(some systems use image/jpg, although official MIME type is image/jpeg)
					case "image/jpeg": // JPG not from within a ClinicalDocument						
					case "image/gif": // GIF not from within a ClinicalDocument						
						{
							var imageBase64 = Convert.ToBase64String(apiResult.Data!);
							
							model.ImageBase64String = imageBase64;

							/*var extension = GetExtension(apiResult.ContentType!);
							var filePath = Path.Combine(patientFolder, documentUniqueId!) + extension;
							System.IO.File.WriteAllBytes(filePath, apiResult.Data!);

							model.DocumentUrl = $"/documents/{patient}/{documentUniqueId}{extension}";
							model.ContentType = apiResult.ContentType;*/

							model.ContentType = apiResult.ContentType;

							model.IsSuccess = true;
						}
						break;
					case "application/fhir+json": // FHIR
						var jsonText = Encoding.UTF8.GetString(apiResult.Data!);

						using (var jsonDoc = JsonDocument.Parse(jsonText))
						{
							model.Html = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
						}

						model.ContentType = apiResult.ContentType;
						model.IsSuccess = true;
						break;
					default:
						model.IsSuccess = false;
						model.ErrorMessage = $"Unsupported content type: {apiResult.ContentType}";
						model.ApiResult = apiResult;
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
			model.ApiResult = apiResult;
			return model;
		}
	}

	public static string GetBaseUrl(HttpRequest request)
	{
		var baseUrl = $"{request.Scheme}://{request.Host.Value}";
#if !DEBUG
        baseUrl = baseUrl.Replace("http://", "https://");  // When inside a pod in the cluster we'll just get http as the scheme, but we need https for consumers of the api
#endif
		return baseUrl;
	}

	public async Task<byte[]> GetPdfFromTxtAsync(string base64Text)
	{
		var baseUrl = GetBaseUrl(Request) + "/api/"; 

		var httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(baseUrl + "mediaProcessorCreator/text2pdf");

		var httpContent = new StringContent(base64Text, Encoding.UTF8, "text/plain");
		var result = await httpClient.PostAsync(httpClient.BaseAddress, httpContent);

		return await result.Content.ReadAsByteArrayAsync();
	}

	//public async Task<byte[]> GetHtmlFromRtfAsync(string base64Text)
	public async Task<string> GetHtmlFromRtfAsync(string base64Text)
	{
		var baseUrl = GetBaseUrl(Request) + "/api/";

		var httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(baseUrl + "mediaProcessorCreator/rtf2html");

		var httpContent = new StringContent(base64Text, Encoding.UTF8, "text/plain");
		var result = await httpClient.PostAsync(httpClient.BaseAddress, httpContent);

		//return await result.Content.ReadAsByteArrayAsync();
		return await result.Content.ReadAsStringAsync();
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

	public IActionResult Download(string file)
	{
		// This method is safe: it prevents directory traversal attacks by canonicalizing the path

		if (string.IsNullOrWhiteSpace(file))
			return BadRequest("Missing file.");

		// Define a fixed root directory for documents
		var rootDir = Path.Combine(Path.GetTempPath(), "documents");

		// Normalize slashes and remove leading "/documents/"
		var relative = file.Replace("/documents/", "")
						   .Replace("\\", "/")
						   .TrimStart('/');

		// Combine the root directory with the relative path
		var combinedPath = Path.Combine(rootDir, relative);

		// Canonicalize the combined path (resolves ../ and symlinks)
		var fullPath = Path.GetFullPath(combinedPath);

		// Ensure the canonical path is still inside rootDir
		var fullRoot = Path.GetFullPath(rootDir);
		if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
		{
			// Attempted directory traversal
			return BadRequest("Invalid file path.");
		}

		// Check existence
		if (!System.IO.File.Exists(fullPath))
			return NotFound();

		// Read file safely
		var bytes = System.IO.File.ReadAllBytes(fullPath);

		return new FileContentResult(bytes, "application/octet-stream")
		{
			FileDownloadName = Path.GetFileName(fullPath)
		};
	}


	private static string GetExtension(string contentType)
	{
		//if (contentType.Contains("/xml") || contentType.Contains("+xml"))
		//{
		//	return ".xml";
		//}
		
		return contentType switch
		{
			//
			// CDA / HL7 Documents
			//
			"text/xml" => ".xml",
			"application/xml" => ".xml",
			"application/hl7-v3+xml" => ".xml",
			"application/x-hl7-v3+xml" => ".xml",
			"application/hl7-cda+xml" => ".xml",
			"application/cda+xml" => ".xml",

			//
			// HL7 v2
			//
			"application/hl7-v2" => ".hl7",
			"text/hl7" => ".hl7",
			"x-application/hl7-v2" => ".hl7",

			//
			// CCR / CCD (old but still seen)
			//
			"application/x-ccr" => ".xml",
			"application/ccr" => ".xml",
			"application/xml+ccr" => ".xml",
			"application/x-continuity-of-care-record+xml" => ".xml",
			"application/xml+ccd" => ".xml",

			//
			// FHIR documents (rare in XDS but supported in MHD)
			//
			"application/fhir+xml" => ".xml",
			"application/fhir+json" => ".json",

			//
			// DICOM
			//
			"application/dicom" => ".dcm",
			"application/dicom+xml" => ".xml",
			"application/dicom+json" => ".json",

			//
			// PDFs
			//
			"application/pdf" => ".pdf",

			//
			// Text formats
			//
			"text/plain" => ".txt",
			"application/txt" => ".txt",
			"text/csv" => ".csv",

			//
			// RTF / Word
			//
			"application/rtf" => ".rtf",
			"text/rtf" => ".rtf",
			"application/msword" => ".rtf",       // many scanners incorrectly use msword for RTF
			"application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
			"application/vnd.ms-word.document.macroEnabled.12" => ".docm",

			//
			// Images
			//
			"image/jpeg" => ".jpg",
			"image/jpg" => ".jpg", // some systems use image/jpg
			"image/png" => ".png",
			"image/gif" => ".gif",
			"image/tiff" => ".tif",
			"image/bmp" => ".bmp",

			//
			// Zip / Binary
			//
			"application/zip" => ".zip",
			"application/octet-stream" => ".bin",

			//
			// Default fallback: use the content type itself
			//
			_ => ".bin" // contentType
		};
	}

}
