using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Io;

namespace HttpPoster.Infrastructure
{
	public class HtmlHelper
	{
		/*
		 * doc:
		 * Receives the HttpResponseMessage and returns an IHtmlDocument. 
		 * GetDocumentAsync uses a factory that prepares a virtual response based on the original HttpResponseMessage. 
		 * For more information, see the AngleSharp documentation.
		 * https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0
		 */
		public static async Task<IHtmlDocument> GetDocumentAsync(HttpResponseMessage response)
		{
			var content = await response.Content.ReadAsStringAsync();
			var document = await BrowsingContext.New()
				.OpenAsync(ResponseFactory, CancellationToken.None);
			return (IHtmlDocument)document;

			void ResponseFactory(VirtualResponse htmlResponse)
			{
				htmlResponse
					.Address(response.RequestMessage.RequestUri)
					.Status(response.StatusCode);

				MapHeaders(response.Headers);
				MapHeaders(response.Content.Headers);

				htmlResponse.Content(content);

				void MapHeaders(HttpHeaders headers)
				{
					foreach (var header in headers)
					{
						foreach (var value in header.Value)
						{
							htmlResponse.Header(header.Key, value);
						}
					}
				}
			}
		}
	}
}
