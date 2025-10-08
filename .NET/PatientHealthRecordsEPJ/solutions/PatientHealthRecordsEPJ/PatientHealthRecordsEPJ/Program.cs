// override XmlException: Resolving of external URIs was prohibited.
using Microsoft.AspNetCore.DataProtection;
using NorskHelsenettUpload.Ihe.Mhd.Interfaces;
using NorskHelsenettUpload.Ihe.Mhd.Services;
using PatientHealthRecords.ApiService;
using PatientHealthRecordsEPJ;
using PatientHealthRecordsEPJ.Services;
using PJD.Services.Persontjenesten;
using StackExchange.Redis;

AppContext.SetSwitch("Switch.System.Xml.AllowDefaultResolver", true);

var builder = WebApplication.CreateBuilder(args);

var apiOptions = builder.Configuration.GetSection("ApiOptions").Get<PhrApiOptions>()!;
builder.Services.AddSingleton(apiOptions);

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
builder.Services.AddSingleton<IOidcClientService, OidcClientService>();

builder.Services.AddScoped<IPhrApiCaller, PhrApiCaller>();
builder.Services.AddScoped<IPhrApiService, PhrApiService>();
builder.Services.AddScoped<IBundleCreator, BundleCreator>();

builder.Services.AddControllersWithViews()
#if DEBUG
	.AddRazorRuntimeCompilation()
#endif
; 

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

builder.Services.AddScoped<OrganizationService>();	// Used to get organization name from Persontjenesten
builder.Services.AddScoped<PersonApiService>();		// Used to get person from Persontjenesten

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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
