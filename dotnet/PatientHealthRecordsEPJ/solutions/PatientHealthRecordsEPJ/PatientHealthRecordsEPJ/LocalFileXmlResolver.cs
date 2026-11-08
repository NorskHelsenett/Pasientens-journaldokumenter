using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Xml;

public class LocalFileXmlResolver : XmlResolver
{
	//private readonly IWebHostEnvironment _webHostEnvironment;

	string currentDirectory; 

	//public LocalFileXmlResolver(IWebHostEnvironment webHostEnvironment)
	//{
	//	_webHostEnvironment = webHostEnvironment;
	//}

	public LocalFileXmlResolver(string currentDirectory)
	{
		this.currentDirectory = currentDirectory;
	}

	public override ICredentials Credentials
	{
		set { /* no-op */ }
	}

	public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
	{
		if (absoluteUri.LocalPath.Contains("KITH"))
		{

		}

		if (absoluteUri.IsFile)
		{
			// Add additional security checks here if necessary
			return new FileStream(absoluteUri.LocalPath, FileMode.Open, FileAccess.Read);
		}

		throw new XmlException("Access to external URIs is prohibited.");
	}

	public override Uri ResolveUri(Uri baseUri, string relativeUri)
	{
		//relativeUri = relativeUri.Replace("/", "\\");

		System.Diagnostics.Debug.WriteLine("RELATIVE: " + relativeUri); 

		//if (Path.IsPathRooted(relativeUri))
		//{
		//	System.Diagnostics.Debug.WriteLine("  IS ROOTED" );
		//}

		if (relativeUri.StartsWith("file://"))
		{
			Console.WriteLine("LocalFileXmlResolver, ResolveUri: " + relativeUri); 

			//relativeUri = relativeUri.Replace("file:///", ""); 

			return new Uri(relativeUri); 
		}

		if (baseUri == null)
		{		
			var path = Path.Combine(currentDirectory, relativeUri);

			//var uri = $"file://{path}";
			var uri = $"{path}";

			//return new Uri(new Uri(currentDirectory), relativeUri);
			return new Uri(uri);
		}
		else
		{
			return new Uri(baseUri, relativeUri);

			var path = Path.GetDirectoryName(baseUri.LocalPath);

			path = Path.Combine(path, relativeUri);

			//var uri = $"file://{path}";
			var uri = $"{path}";

			return new Uri(uri);

		}		
	}
}
