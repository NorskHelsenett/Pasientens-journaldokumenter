using IdentityModel.Client;
using IdentityModel.OidcClient.DPoP;
using System.Net.Http.Headers;

public class ApiResult
{
	public bool IsSuccess { get; set; }
	public int StatusCode { get; set; }
	public string ErrorMessage { get; set; }

	public byte[]? Data { get; set; }
	public string Body { get; set; }
	public string? ContentType { get; set; }

	public int NorsCount { get; set; } // Antall sperrede dokumenter, returnert som X-SecurityLabel-Nors-Count header parameter
}

public class ApiCaller
{
	// Call Get Document References or Get Document api endpoints, with accesstoken and DPoP 
	public static async Task<ApiResult> CallApi(string method, 
		string url, 
		string accessToken, 
		bool document, 
		List<KeyValuePair<string, string>> headerKeyValues = null!, 
		List<string> acceptHeaderValues =  null!,
		FormUrlEncodedContent formUrlEncodedContent = null!)
	{
		var request = method == "GET" ? new HttpRequestMessage(HttpMethod.Get, url) : new HttpRequestMessage(HttpMethod.Post, url);

		if (formUrlEncodedContent != null)
		{
			request.Content = formUrlEncodedContent;
		}

		var proofRequest = new DPoPProofRequest
		{
			Method = request.Method.ToString(),
			Url = request.GetDPoPUrl(),
			// This binds the access token to the DPoP proof:
			AccessToken = accessToken,
		};

		// Set the DPoP proof, we use the same key for this as for the client assertion:
		var proof = new DPoPProofTokenFactory(Constants.JwkPrivateKey).CreateProofToken(proofRequest);

		request.SetDPoPToken(accessToken, proof.ProofToken);

		if (headerKeyValues is not null)
		{
			foreach (var keyValue in headerKeyValues)
			{
				request.Headers.Add(keyValue.Key, keyValue.Value);
			}
		}

		if (acceptHeaderValues is not null)
		{
			foreach (var mediaType in acceptHeaderValues)
			{
				var mediaTypeParts = mediaType.Split(';', 2); // Split media type and parameters
				var mediaTypeValue = new MediaTypeWithQualityHeaderValue(mediaTypeParts[0].Trim());

				if (mediaTypeParts.Length > 1) // If there are parameters
				{
					var parameters = mediaTypeParts[1].Split(';');
					foreach (var parameter in parameters)
					{
						var keyValue = parameter.Split('=');
						if (keyValue.Length == 2)
						{
							mediaTypeValue.Parameters.Add(new NameValueHeaderValue(keyValue[0].Trim(), keyValue[1].Trim()));
						}
					}
				}

				request!.Headers.Accept.Add(mediaTypeValue);
			}
		}

		var handler = new HttpClientHandler
		{
			// Handle staging certificate from Let's Encrypt used by PHR API
			ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
		};

		var httpClient = new HttpClient(handler);
		var response = await httpClient.SendAsync(request);

		if (response.IsSuccessStatusCode)
		{
			if (!document)
			{
				int norsCount = 0;

				if (response.Headers.TryGetValues("NHN-SecurityLabel-Nors-Count", out var norsValues))
				{
					norsCount = Int32.Parse(norsValues.FirstOrDefault()!);
				}

				var responseBody = await response.Content.ReadAsStringAsync();

				return new ApiResult()
				{
					IsSuccess = true,
					StatusCode = (int)response.StatusCode,
					Body = responseBody,
					ContentType = response.Content.Headers.ContentType!.ToString(),
					NorsCount = norsCount,
				};
			}
			else
			{
				var responseBytes = await response.Content.ReadAsByteArrayAsync();

				return new ApiResult()
				{
					IsSuccess = true,
					StatusCode = (int)response.StatusCode,
					Data = responseBytes,
					ContentType = response.Content.Headers.ContentType!.ToString(),
				};
			}
		}
		else
		{
			var responseBody = await response.Content.ReadAsStringAsync();

			return new ApiResult() 
			{ 
				IsSuccess = false, 
				StatusCode = (int)response.StatusCode,
				Body = responseBody	
			};
		}
	}
}