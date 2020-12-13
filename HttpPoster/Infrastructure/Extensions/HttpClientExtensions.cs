using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Xunit;

namespace HttpPoster.Infrastructure.Extensions
{
	/*
	 * doc:
	 * SendAsync extension methods for the HttpClient compose an HttpRequestMessage 
	 * and call SendAsync(HttpRequestMessage) to submit requests to the SUT. 
	 * Overloads for SendAsync accept the HTML form (IHtmlFormElement) and the following:
		Submit button of the form (IHtmlElement)
		Form values collection (IEnumerable<KeyValuePair<string, string>>)
		Submit button (IHtmlElement) and form values (IEnumerable<KeyValuePair<string, string>>)
	*	https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0
	*/
	public static class HttpClientExtensions
	{
		public static Task<HttpResponseMessage> SendAsync(
			this HttpClient client,
			IHtmlFormElement form,
			IHtmlElement submitButton)
		{
			return client.SendAsync(form, submitButton, new Dictionary<string, string>());
		}

		public static Task<HttpResponseMessage> SendAsync(
			this HttpClient client,
			IHtmlFormElement form,
			IEnumerable<KeyValuePair<string, string>> formValues)
		{
			var submitElement = Assert.Single(form.QuerySelectorAll("[type=submit]"));
			var submitButton = Assert.IsAssignableFrom<IHtmlElement>(submitElement);

			return client.SendAsync(form, submitButton, formValues);
		}

		public static Task<HttpResponseMessage> SendAsync(
			this HttpClient client,
			IHtmlFormElement form,
			IHtmlElement submitButton,
			IEnumerable<KeyValuePair<string, string>> formValues)
		{
			Assert.NotNull(form);
			Assert.NotNull(submitButton);
			
			foreach (KeyValuePair<string, string> kvp in formValues)
			{
				AngleSharp.Dom.IElement input = form[kvp.Key];
				Assert.NotNull(input);

				IHtmlInputElement element = Assert.IsAssignableFrom<IHtmlInputElement>(input);
				element.Value = kvp.Value;
			}

			var submit = form.GetSubmission();
			Assert.NotNull(submit);
			var target = (Uri)submit.Target;
			if (submitButton.HasAttribute("formaction"))
			{
				var formaction = submitButton.GetAttribute("formaction");
				target = new Uri(formaction, UriKind.Relative);
			}
			var submission = new HttpRequestMessage(new HttpMethod(submit.Method.ToString()), target)
			{
				Content = new StreamContent(submit.Body)
			};

			foreach (var header in submit.Headers)
			{
				submission.Headers.TryAddWithoutValidation(header.Key, header.Value);
				submission.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			return client.SendAsync(submission);
		}
	}
}
