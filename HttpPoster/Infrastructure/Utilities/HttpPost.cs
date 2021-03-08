using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using HttpPoster.Infrastructure.Extensions;

namespace HttpPoster.Infrastructure.Utilities
{
	public class HttpPost
	{ //Wrapper for HttpClient
		public HttpClient Client { get; set; } = new HttpClient();
		public HttpResponseMessage LastResponse { get; set; }
		public string RootUri { get; set; } = "http://localhost:8080";

		public HttpPost() { }
		public HttpPost(string rootUrl)
		{
			this.RootUri = rootUrl.TrimEnd('/');
		}

		public async Task<HttpResponseMessage> Post(string pageUri, Dictionary<string, string> postFormValue, string expectedRedirectedPage = null)
		{
			pageUri = $"{RootUri}/{pageUri}";
			var content = new FormUrlEncodedContent(postFormValue);

			//Act
			var response = await Client.PostAsync(pageUri, content);
			LastResponse = response;

			//Assert
			//string html = await response.Content.ReadAsStringAsync();
			string requestUri = response.RequestMessage.RequestUri.ToString();
			if (expectedRedirectedPage != null 
				&& (RootUri + '/' + expectedRedirectedPage).ToLower() != requestUri.ToLower())
				throw new Exception($"Unexpected rediction of page: {requestUri}");

			if (response.StatusCode != System.Net.HttpStatusCode.OK)
				throw new HttpRequestException(await response.Content.ReadAsStringAsync());

			return response;
		}

		public async Task<HttpResponseMessage> PostBackWithHtmlForm(string pageUri, CssSelectorForDom selector, Dictionary<string, string> postFormValue, string expectedRedirectedPage = null)
		{
			var response = await Client.GetAsync($"{RootUri}/{pageUri}");
			var content = await HtmlHelper.GetDocumentAsync(response);

			IHtmlFormElement form = (IHtmlFormElement)content.QuerySelector(selector.Form);
			IHtmlButtonElement btnSubmit = (IHtmlButtonElement)content.QuerySelector(selector.SubmitButton);

			//Act
			HttpResponseMessage result = await Client.SendAsync(form, btnSubmit, postFormValue);
			LastResponse = result;

			//Assert

			string requestUri = result.RequestMessage.RequestUri.ToString();
			if (expectedRedirectedPage != null)
			{
				string expectedUri = (RootUri + '/' + expectedRedirectedPage).ToLower();
				if (expectedUri != requestUri.ToLower())
					throw new Exception($"Unexpected rediction of page: {requestUri}");
			}

			if (response.StatusCode != System.Net.HttpStatusCode.OK)
				throw new HttpRequestException(await response.Content.ReadAsStringAsync());

			/*Success might not be determined correctly 
			 * due to Server still response Http status 200 OK
			 * but use JavaScript or Dom UI element to indicate error instead.
			 */
			return result;
		}

		public class CssSelectorForDom
		{ //for targeting DOM elements via CSS selector string
			public string Form { get; set; } = "form[action]";
			public string SubmitButton { get; set; } = "form button[type='submit']";
		}
	}
}
