// override XmlException: Resolving of external URIs was prohibited.
using Hl7.Fhir.ElementModel.Types;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using NorskHelsenettUpload.Ihe.Mhd.Interfaces;
using NorskHelsenettUpload.Ihe.Mhd.Services;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ;
using PatientHealthRecordsEPJ.DokumentlagerTest;
using PatientHealthRecordsEPJ.Services;
using StackExchange.Redis;

const long OneGb = 1L * 1024 * 1024 * 1024;

AppContext.SetSwitch("Switch.System.Xml.AllowDefaultResolver", true);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
	options.Limits.MaxRequestBodySize = OneGb;
});

// Multipart/form-data (IFormFile uploads)
builder.Services.Configure<FormOptions>(options =>
{
	options.MultipartBodyLengthLimit = OneGb;

	// Optional: if you expect very long file names / lots of form keys, etc.
	// options.ValueLengthLimit = int.MaxValue;
	// options.MultipartHeadersLengthLimit = int.MaxValue;
});

var apiOptions = builder.Configuration.GetSection("ApiOptions").Get<PhrApiOptions>()!;

builder.Services.AddSingleton(apiOptions);
builder.Services.AddSingleton(apiOptions.Environments);

var epjOptions = builder.Configuration.GetSection("PhrEpjOptions").Get<PhrEpjOptions>()!;
builder.Services.AddSingleton(epjOptions);

builder.Services.AddScoped<IPhrApiEnvironmentProvider, PhrApiEnvironmentProvider>();

builder.Services
	.AddOptions<HelseIdOptions>()
	.BindConfiguration(HelseIdOptions.SectionKey);

#if !DEBUG
	var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

	var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

	redisConnectionString += ",password=" + redisPassword; 

	Console.WriteLine("redisConnectionString:" + redisConnectionString);

	builder.Services.AddDataProtection()
		.SetApplicationName("PatientHealthRecordsEPJ")
		.PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(redisConnectionString), "DataProtection-Keys");
		
	// Add Redis distributed cache
	builder.Services.AddStackExchangeRedisCache(options =>
	{
		options.Configuration = redisConnectionString; 
		options.InstanceName = "PatientHealthRecordsEPJ"; 
	});
#endif

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IHelseIdDiscoveryDocumentCache, HelseIdDiscoveryDocumentCache>();
builder.Services.AddSingleton<IOidcClientService, OidcClientService>();

builder.Services.AddScoped<IUserTokenService, UserTokenService>();

builder.Services.AddScoped<IPhrApiCaller, PhrApiCaller>();
builder.Services.AddScoped<IPhrApiService, PhrApiService>();
builder.Services.AddScoped<IBundleCreator, BundleCreator>();
builder.Services.AddScoped<ICreateDocumentService, CreateDocumentService>();

builder.Services.AddControllersWithViews()
#if DEBUG
	.AddRazorRuntimeCompilation()
#endif
;

builder.Services.AddDistributedMemoryCache();

// Services for Dokumentlager tests
builder.Services.AddSingleton<ITestRunStore, InMemoryTestRunStore>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddScoped<IDokumentlagerTestRunner, DokumentlagerTestRunner>();
builder.Services.AddHostedService<QueuedHostedService>();
// End of services for Dokumentlager tests

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
	{
		//options.IdleTimeout = TimeSpan.FromSeconds(20); 
		//options.IdleTimeout = TimeSpan.FromMinutes(30); 
		options.IdleTimeout = TimeSpan.FromHours(8); 		
		options.Cookie.HttpOnly = true; // Make the session cookie HttpOnly
		options.Cookie.IsEssential = true; // Mark the session cookie as essential
		options.Cookie.Name = "PatientHealthRecordsEPJ"; 		
	});

// Backend NHN Persontjenesten
//builder.Services.AddHttpClient<PersonController>("PersontjenestenAPI", client => client.BaseAddress = new Uri(builder.Configuration["PersontjenestenEndpoints:test"]!));

// Backend NHN Persontjenesten
builder.Services.AddHttpClient("OrganizationAPI", client => client.BaseAddress = new Uri(builder.Configuration["OrgRegEndpoints:Dev"]!))
	.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
	{
		// Disable SSL certificate validation
		ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
	})
	.UseHelseIdDPoP(configureClient => 
	{
		configureClient.Audience = "nhn:orgreg";
		configureClient.Scopes = "nhn:orgreg/read";

		// PrivateJwk, ClientId and TokenEndpoint is the same as for Persontjenesten
	});

builder.Services.AddHttpClient("PersonApiClient", client =>
{
	client.DefaultRequestHeaders.Add("api-version", "3");
	client.BaseAddress = new Uri(builder.Configuration["PersontjenestenEndpoints:test"]!);
}).UseHelseIdDPoP();

builder.Services.AddHttpClient("KjernejournalLoginApiClient", client =>
{	
	client.BaseAddress = new Uri(builder.Configuration["KjernejournalLoginEndpoints:Test"]!);
}).UseHelseIdDPoP(configureClient =>
{
	configureClient.Audience = "nhn:kjernejournal";
	configureClient.Scopes = "nhn:kjernejournal/innlogging nhn:kjernejournal/tillitsrammeverk";

	// PrivateJwk, ClientId and TokenEndpoint is the same as for Persontjenesten
});

//builder.Services.AddHttpClient("CreateDocumentsWithReferenceApiClient", client => client.BaseAddress = new Uri(builder.Configuration["OrgRegEndpoints:Dev"]!))
builder.Services.AddHttpClient("ProvideDocumentBundleApiClient", client => client.BaseAddress = new Uri("https://api.pjd.test.nhn.no"))
	.UseHelseIdDPoP(configureClient =>
	{
		//configureClient.Audience = "nhn:phr";
		//configureClient.Scopes = "nhn:phr/mhd/create-documents-with-reference";

		configureClient.Audience = "nhn:phr-repo";
		configureClient.Scopes = "nhn:phr-repo/mhd/create nhn:phr-repo/delete-docs-and-refs";

		// PrivateJwk, ClientId and TokenEndpoint is the same as for Persontjenesten
	});

builder.Services.AddHttpClient(DokumentlagerTestRunner.DokumentlagerTestHttpClientName, client =>
{
	// Configure the HttpClient instance if needed (e.g., set base address)
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
	var clientHandler = new HttpClientHandler
	{
		// (Optional) Disable SSL certificate validation for development purposes
		ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
		{
			return true; 
		}
	};	

	return clientHandler;
});

builder.Services.AddScoped<OrganizationService>();	// Used to get organization name from Persontjenesten
builder.Services.AddScoped<PersonApiService>();     // Used to get person from Persontjenesten
builder.Services.AddScoped<ProvideDocumentBundleApiService>();     // Used to provide document bundle to PHR API
builder.Services.AddScoped<KjernejournalLoginApiService>();     // Used to handle Kjernejournal login

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseSession();

app.UseMiddleware<SessionActivityMiddleware>();

app.UseHttpsRedirection();

var tempDocs = Path.Combine(Path.GetTempPath(), "documents");
Directory.CreateDirectory(tempDocs);

app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(tempDocs),
	RequestPath = "/documents"
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
