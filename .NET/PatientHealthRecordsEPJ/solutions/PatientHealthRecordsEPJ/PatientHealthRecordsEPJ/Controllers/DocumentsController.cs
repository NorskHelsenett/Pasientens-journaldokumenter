using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ.Models;
using PdfSharpCore;
using System.Diagnostics;
using System.Text.Json;
using XcaXds.Commons.Extensions;

namespace PatientHealthRecordsEPJ.Controllers;

public class DocumentsController : BaseController
{
	private readonly ILogger<HomeController> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private ISession session => _httpContextAccessor.HttpContext!.Session;
	private readonly IMemoryCache _memoryCache;
	private readonly IOidcClientService _oidcClientService;
	private readonly IPhrApiEnvironmentProvider _phrApiEnvironmentProvider;

	SessionData sessionLoginResult;

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

	public DocumentsController(
		IOptions<SessionOptions> sessionOptions,
		ILogger<HomeController> logger,
		IHttpContextAccessor httpContextAccessor,
		IMemoryCache memoryCache,
		IPhrApiEnvironmentProvider phrApiEnvironmentProvider,
		IOidcClientService oidcClientService) : base(sessionOptions)
	{
		_logger = logger;
		_httpContextAccessor = httpContextAccessor;
		_memoryCache = memoryCache;
		_phrApiEnvironmentProvider = phrApiEnvironmentProvider;
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

	public async Task<IActionResult> RefreshToken()
	{
		var redirectUrl = GetRedirectUrl();

		await _oidcClientService.InitializeAsync(redirectUrl);

		var loginResult = session.SessionData();

		var result = await _oidcClientService.RefreshTokenAsync(loginResult.RefreshToken);

		if (result.IsError)
		{
			throw new Exception($"{result.Error}: {result.ErrorDescription}");
		}

		loginResult.AccessToken = result.AccessToken;
		loginResult.RefreshToken = result.RefreshToken;
		loginResult.SaveToSession(session);

		return Ok(result.AccessToken);
	}

	public async Task<IActionResult> Index(string filter, [FromQuery] string query, int page = 1, int pageSize = DEFAULT_PAGE_SIZE, string sort = "-date", bool? useCaching = null)
	{
		var sessionData = session.SessionData();

		if (sessionData.AccessToken == null)
		{
			return Redirect("/");
		}

		await RefreshToken();

		sessionLoginResult = session.SessionData();

		if (!sessionLoginResult.IsAuthenticated)
			return Redirect("/");

		if (useCaching != null)
		{
			sessionLoginResult.UseCaching = useCaching.Value;
			sessionLoginResult.SaveToSession(session);
		}

		var model = await GetDocumentReferences(page, pageSize, sort: sort, query: query);

		model.UseCaching = sessionLoginResult.UseCaching;

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
		}

		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-source-system", "PJD Test EPJ"));
		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()));

		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-bypass-cache", sessionLoginResult.UseCaching ? "false" : "true"));

		var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

		var apiUrl = _apiEnvironment.GetDocumentReferencesApiUrl;

		var urlencodedContent = new FormUrlEncodedContent([
			new("patient.identifier", sessionLoginResult.SourcePatientInfo?.NIN),
			new("status", "current"),
			new("_count", pageSize.ToString()),
			new("_page", pageIndex.ToString()),
			new("_sort", sort),
			// If _include contains DocumentReference:relatesTo, get associations. Note: Will make an extra call to XDS for associations and slow down UI rendering.
			// OK for Test EPJ so that we can show associations in the UI in the "Info" column. 
			// In production, consider calling a separate API (GetDocumentAssociations) to get associations only when user requests it for a specific document.			
			new("_include", "DocumentReference:relatesTo") 
		]);

		var apiResult = await ApiCaller.CallApi(HttpMethod.Post,
			apiUrl,
			sessionLoginResult.AccessToken!,
			document: false,
			headerKeyValues: headerKeyValues,
			acceptHeaderValues: ["application/fhir+json; fhirVersion=4.0"], // Only empty accept header, or 'application/fhir+json' is allowed				
			content: urlencodedContent
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
				model.IsRetrievedFromCache = apiResult.IsRetrievedFromCache;

				var operationOutcome = bundle.GetResources().OfType<OperationOutcome>().FirstOrDefault();

				if (operationOutcome != null)
				{
					foreach (var issue in operationOutcome.Issue)
					{
						if (issue.Severity == OperationOutcome.IssueSeverity.Error)
						{
							var firstCoding = issue.Details.Coding.FirstOrDefault();

							if (firstCoding != null)
							{
								var system = firstCoding.System;
								var code = firstCoding.Code;
								var display = firstCoding.Display;
							}

							if (issue.Diagnostics != null)
							{
								var unavailableLocation = JsonSerializer.Deserialize<UnavailableLocation>(issue.Diagnostics);

								model.UnavailableLocations.Add(unavailableLocation!);
							}
						}
					}
				}

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
			model.ErrorMessage = "Could not retrieve documents";
			model.ApiResult = apiResult;
			model.Pagination = new Pagination(); // To avoid NullReferenceException in onChangePageSize() and onChangePage() in Index.cshtml
			return model;
		}
	}

	public async Task<IActionResult> DeleteDocument(string documentUniqueId, string homeCommunityId)	
	{
		var sessionData = session.SessionData();

		documentUniqueId = documentUniqueId.NoUrn();
		homeCommunityId = homeCommunityId.NoUrn(); 

		var headerKeyValues = new List<KeyValuePair<string, string>>();
		
		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-source-system", "PJD Test EPJ"));
		headerKeyValues.Add(new KeyValuePair<string, string>("nhn-event-id", Guid.NewGuid().ToString()));

		var _apiEnvironment = _phrApiEnvironmentProvider.GetCurrentEnvironment();

		var apiUrl = $"{_apiEnvironment.DeleteDocumentUrl}/{documentUniqueId}?homeCommunityId={homeCommunityId}";
		
		var apiResult = await ApiCaller.CallApi(HttpMethod.Delete,
			apiUrl,
			sessionData.AccessToken!,
			document: false,
			headerKeyValues: headerKeyValues
			);

		if (apiResult.IsSuccess)
		{
			try
			{
				var json = apiResult.Body;				

				var parser = new FhirJsonParser();

				var operationOutcome = parser.Parse<OperationOutcome>(json);

				if (operationOutcome != null)
				{
					foreach (var issue in operationOutcome.Issue)
					{
						if (issue.Severity == OperationOutcome.IssueSeverity.Error)
						{
							var firstCoding = issue.Details.Coding.FirstOrDefault();

							if (firstCoding != null)
							{
								var system = firstCoding.System;
								var code = firstCoding.Code;
								var display = firstCoding.Display;
							}

							if (issue.Diagnostics != null)
							{
								var unavailableLocation = JsonSerializer.Deserialize<UnavailableLocation>(issue.Diagnostics);

								//model.UnavailableLocations.Add(unavailableLocation!);
							}
						}
					}
				}				
				
				//return model;
			}
			catch (Hl7.Fhir.Serialization.DeserializationFailedException e)
			{
				foreach (var ex in e.Exceptions)
				{
					Debug.WriteLine(ex.Message);
				}

				//model.IsSuccess = false;

				//return model;
			}
		}
		else
		{
			/*model.IsSuccess = false;
			model.ErrorMessage = "Could not retrieve documents";
			model.ApiResult = apiResult;
			model.Pagination = new Pagination(); // To avoid NullReferenceException in onChangePageSize() and onChangePage() in Index.cshtml
			return model; */
		}

		return Redirect("/documents"); 
	}
}
