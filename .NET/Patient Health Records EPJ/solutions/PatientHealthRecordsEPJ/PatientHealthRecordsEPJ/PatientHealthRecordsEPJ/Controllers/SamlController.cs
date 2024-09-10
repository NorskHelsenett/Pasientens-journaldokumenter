using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml;

namespace PatientHealthRecordsEPJ.Controllers
{
	public class SamlAssertionViewModel
	{
        public bool IsAuthenticated { get; set; }
		public bool IsSuccess { get; set; }

		public ApiResult ApiResult { get; set; } = default!;

		public string ErrorMessage { get; set; } = default!;
		public string SamlAssertion { get; set; } = default!; 
	}

	public class SamlController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession session => _httpContextAccessor.HttpContext!.Session;
        private readonly IMemoryCache _memoryCache;
        private readonly ApiUrlSettings _apiUrlSettings;
		                
        SessionLoginResult sessionLoginResult; 
		
		public SamlController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache, ApiUrlSettings apiUrlSettings)
		{
			_logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            _apiUrlSettings = apiUrlSettings;
        }

		public async Task<IActionResult> ShowSamlAssertion()
		{
			sessionLoginResult = session.SessionLoginResult();			

			if (!sessionLoginResult.IsAuthenticated)
				return View(new SamlAssertionViewModel() { IsAuthenticated = false});
		
			var model = await GetSamlAssertion(sessionLoginResult.PatientId);			

			return View(model);
		}

		private async Task<SamlAssertionViewModel> GetSamlAssertion(string patientId)
        {
			SamlAssertionViewModel model = new SamlAssertionViewModel();

			var apiUrl = _apiUrlSettings.GetSamlAssertionUrl + $"?patient.identifier={sessionLoginResult.PatientId}";
		
			var apiResult = await ApiCaller.CallApi("POST", apiUrl, sessionLoginResult.AccessToken!, document: false);			
			
			if (apiResult.IsSuccess)
            {
                try
                {
                    model.SamlAssertion = PrettyPrintXml(apiResult.Body);  

                    model.IsSuccess = true;					

					return model;
                }
                catch (Hl7.Fhir.Serialization.DeserializationFailedException e)
                {					
					foreach (var ex in e.Exceptions)
                    {
                        Debug.WriteLine(ex.Message);
                    }

					model.IsSuccess = false;

					return model;
				}
            }
            else
            {
                model.IsSuccess = false;
                model.ErrorMessage = $"Could not get saml assertion for user {sessionLoginResult.Name} ({sessionLoginResult.PersonId}) and patient: {patientId}";
				model.ApiResult = apiResult;				
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
