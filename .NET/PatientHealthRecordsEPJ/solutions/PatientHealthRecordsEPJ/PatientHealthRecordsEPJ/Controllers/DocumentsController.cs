using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PatientHealthRecordsEPJ.Models;
using System.Diagnostics;
using System.Text.Json;

namespace PatientHealthRecordsEPJ.Controllers
{
	public class DocumentsController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession session => _httpContextAccessor.HttpContext!.Session;
        private readonly IMemoryCache _memoryCache;
        private readonly ApiUrlSettings _apiUrlSettings;
		                
        SessionLoginResult sessionLoginResult; 

		const int DEFAULT_PAGE_SIZE = 15;
		//const int DEFAULT_PAGE_SIZE = 1000;

		/*
		private const string SORTEXPRESSION = "Date";

		public enum SortBy
		{
			Date = 0,     
            Name = 1, 
            SecurityLabel = 2,  // Nekting
			Category = 3,       // Dokumenttype
			Type = 4,           // Undertype
			Department = 5,     // Enhet/avdeling
			Organization = 6    // Institusjon
		}*/

		public DocumentsController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache, ApiUrlSettings apiUrlSettings)
		{
			_logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            _apiUrlSettings = apiUrlSettings;
        }
	
		public async Task<IActionResult> Index(string filter, [FromQuery] string query, int page = 1, int pageSize = DEFAULT_PAGE_SIZE, string sort = "-date")
		{           
			sessionLoginResult = session.SessionLoginResult();			

			if (!sessionLoginResult.IsAuthenticated)
				return Redirect("/");			

			var model = await GetDocumentReferences(page, pageSize, sort: sort, query: query);			

			return View(model);
		}

		private async Task<GetDocumentReferencesViewModel> GetDocumentReferences(
            int pageIndex = 1,			
			int pageSize = DEFAULT_PAGE_SIZE,
			string sort = "-date",		
			string query = ""
			)
        {
            GetDocumentReferencesViewModel model = new GetDocumentReferencesViewModel();
			
			var headerKeyValues = new List<KeyValuePair<string, string>>();

			if (sessionLoginResult.DocumentListScopeId == (int)DocumentListScopeEnum.Kjernejournalforskriften)
			{
				// Only send access basis if using scope 'nhn:phr/mhd/read-documentreferences/kjernejournalforskriften'
				headerKeyValues.Add(new KeyValuePair<string, string>("nhn-access-basis", sessionLoginResult.GrunnlagId ?? string.Empty));
				headerKeyValues.Add(new KeyValuePair<string, string>("nhn-source-system", "PJD Test EPJ 1.0"));
				headerKeyValues.Add(new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()));
			}

			var apiUrl = _apiUrlSettings.GetDocumentReferencesApiUrl; 

			var urlencodedContent = new FormUrlEncodedContent([
				new("patient.identifier", sessionLoginResult.PatientId),
				new("status", "current"),
				new("_count", pageSize.ToString()),
				new("_page", pageIndex.ToString()),
				new("_sort", sort)
			]);

			var apiResult = await ApiCaller.CallApi("POST", 
				apiUrl, 
				sessionLoginResult.AccessToken!, 
				document: false, 
				headerKeyValues: headerKeyValues,
				acceptHeaderValues: ["application/fhir+json"], // Only empty accept header, or 'application/fhir+json' is allowed	
				formUrlEncodedContent: urlencodedContent
				);
			
			if (apiResult.IsSuccess)
            {
                try
                {
                    var json = apiResult.Body;

                    var serializerOptions = new JsonSerializerOptions().ForFhir(ModelInfo.ModelInspector).Pretty();
                    var bundle = JsonSerializer.Deserialize<Bundle>(json, serializerOptions);

                    //model.Entries = new X.PagedList.PagedList<Bundle.EntryComponent>(bundle!.Entry, pageIndex, pageSize);

                    model.Bundle = bundle;
                    model.NorsCount = apiResult.NorsCount;

                    //model.BlockedCount = bundle.entr // Hvordan gjør vi det når rest api bare returnerer et subset? 

                    model.Pagination = Pagination.Create(bundle.Total ?? 0, pageIndex, pageSize);

                    model.SortExpression = sort;

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
                model.ErrorMessage = "Could not retreive documents";
				model.ApiResult = apiResult;
				model.Pagination = new Pagination(); // To avoid NullReferenceException in onChangePageSize() and onChangePage() in Index.cshtml
				return model; 
            }            
        }       
    }
}
