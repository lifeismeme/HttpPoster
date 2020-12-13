using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using HttpPoster.Infrastructure.Extensions;
using Xunit;

namespace HttpPoster.Infrastructure.Utilities
{
	public class HttpPost
	{ //Wrapper for HttpClient
		public HttpClient Client { get; set; } = new HttpClient();
		public HttpResponseMessage LastResponse { get; set; }
		public string RootUrl { get; set; } = "http://localhost:8080";

		public HttpPost() { }
		public HttpPost(string rootUrl)
		{
			this.RootUrl = rootUrl;
		}

		public async Task<HttpResponseMessage> Post(string uri, Dictionary<string, string> postFormValue, string expectedRedirectedPage = null)
		{
			uri = $"{RootUrl}/{uri}";
			var content = new FormUrlEncodedContent(postFormValue);

			//Act
			var response = await Client.PostAsync(uri, content);
			LastResponse = response;

			//Assert
			//string html = await response.Content.ReadAsStringAsync();
			string requestUri = response.RequestMessage.RequestUri.ToString();
			if (expectedRedirectedPage != null && expectedRedirectedPage.ToLower() != requestUri)
				throw new Exception($"Unexpected rediction of page: {requestUri}");

			response.EnsureSuccessStatusCode();

			return response;
		}

		public async Task<HttpResponseMessage> PostBackWithHtmlForm(string uri, Dictionary<string, string> postFormValue, string expectedRedirectedPage = null)
		{
			var response = await Client.GetAsync($"{RootUrl}/{uri}");
			var content = await HtmlHelper.GetDocumentAsync(response);

			var form = (IHtmlFormElement)content.QuerySelector("form[action='/Sales/CreateSales']");
			var btn = (IHtmlButtonElement)content.QuerySelector("button[type='submit']");

			//Act
			HttpResponseMessage result = await Client.SendAsync(form, btn, postFormValue);
			LastResponse = result;

			//Assert

			string requestUri = response.RequestMessage.RequestUri.ToString();
			if (expectedRedirectedPage != null && expectedRedirectedPage.ToLower() != requestUri)
				throw new Exception($"Unexpected rediction of page: {requestUri}");

			response.EnsureSuccessStatusCode();
			/*Success might not be determined correctly 
			 * due to Server still response Http status 200 OK
			 * but use JavaScript or Dom UI element to indicate error instead.
			 */
			return result;
		}
	}
}
