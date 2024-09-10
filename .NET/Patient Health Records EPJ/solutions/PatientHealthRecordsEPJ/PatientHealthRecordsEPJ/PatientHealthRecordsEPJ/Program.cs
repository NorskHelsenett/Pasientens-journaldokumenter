// override XmlException: Resolving of external URIs was prohibited.
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

AppContext.SetSwitch("Switch.System.Xml.AllowDefaultResolver", true);

var builder = WebApplication.CreateBuilder(args);

var apiUrlSettings = builder.Configuration.GetSection("ApiUrls").Get<ApiUrlSettings>()!;
builder.Services.AddSingleton(apiUrlSettings);

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


builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
	{
		options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the session timeout
		options.Cookie.HttpOnly = true; // Make the session cookie HttpOnly
		options.Cookie.IsEssential = true; // Mark the session cookie as essential
		options.Cookie.Name = "PatientHealthRecordsEPJ"; 
	}); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
