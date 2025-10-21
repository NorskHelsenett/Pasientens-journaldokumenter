namespace ConsoleUploadDocument;

// This class functions as a makeshift web host, getting the redirect response from the browser.
// If the status code is 200, the query string containing the authorization code is set on the
// task completion source, which can then be used by the program to obtain tokens from HelseID
public class LoopbackHttpListener : IDisposable
{
    private const int DefaultTimeout = 60 * 5; // 5 mins (in seconds)
    private readonly IWebHost _host;
    //private readonly IHost _host;
    private readonly TaskCompletionSource<string> _source = new ();

    public LoopbackHttpListener(int port)
    {
        //var url = $"http://localhost:{port}/";
        var url = $"https://localhost:{port}/";

        _host = new WebHostBuilder()
            // Set 'PreventHostingStartupKey' to avoid exception
            // System.InvalidOperationException: 'Unable to resolve service for type 'Microsoft.AspNetCore.Routing.EndpointDataSource' while attempting to activate
            // 'Microsoft.AspNetCore.Mvc.ApiExplorer.EndpointMetadataApiDescriptionProvider'.'  (Not sure why it started to appear, but ChatGPT said this line fixes it and it does)
            .UseSetting(WebHostDefaults.PreventHostingStartupKey, "true")             
            .UseKestrel()
            .UseUrls(url)            
            .Configure(Configure)
            .Build();
       
        _host.Start();
    }

    public void Dispose()
    {
        Task.Run(async () =>
        {
            await Task.Delay(500);
            _host.Dispose();
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.Run(async ctx =>
        {
            if (ctx.Request.Method == "GET")
            {
                await SetResultAsync(ctx.Request.QueryString.Value, ctx);
            }
            else
            {
                ctx.Response.StatusCode = 405;
            }
        });
    }

    private async Task SetResultAsync(string value, HttpContext ctx)
    {
        _source.TrySetResult(value);

        try
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "text/html";
            await ctx.Response.WriteAsync("<h1>You can now return to the application.</h1>");
            await ctx.Response.Body.FlushAsync();
        }
        catch
        {
            ctx.Response.StatusCode = 400;
            ctx.Response.ContentType = "text/html";
            await ctx.Response.WriteAsync("<h1>Invalid request.</h1>");
            await ctx.Response.Body.FlushAsync();
        }
    }

    public Task<string> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
    {
        Task.Run(async () =>
        {
            await Task.Delay(timeoutInSeconds * 1000);
            _source.TrySetCanceled();
        });

        return _source.Task;
    }
}
