using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HttpPoster.Infrastructure.Extensions;
using HttpPoster.Infrastructure.Utilities;
using Xunit;

namespace Tests
{
	public class UnitTest1
	{
		private static readonly string HOST_ADDRESS = "localhost:52401";
		public static readonly string ROOT_URI = $"http://{HOST_ADDRESS}";

		private async Task<HttpPost> Login()
		{
			var post = new Dictionary<string, string>()
			{
				{ "Email", "abc@email" },
				{ "Password", "1234567" }
			};
			string loginPage = "login";
			string expectedRedirectedPage = "home";
			var Poster = new HttpPost(ROOT_URI);

			await Poster.Post(loginPage, post, expectedRedirectedPage);

			return Poster;
		}

		[Fact]
		public async void Login_FormValue_SuccessWithRedirectedAsync()
		{
			var post = new Dictionary<string, string>()
			{
				{ "Email", "abc@email" },
				{ "Password", "1234567" }
			};
			string loginPage = "login";
			string expectedRedirectedPage = "home";
			var Poster = new HttpPost(ROOT_URI);

			//Act
			var response = await Poster.Post(loginPage, post, expectedRedirectedPage);

			//Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal(response.RequestMessage.RequestUri.ToString().ToLower(), (ROOT_URI + '/' + expectedRedirectedPage).ToLower());
		}

		[Fact]
		public async void LoginWithForm_FormValue_SuccessWithRedirectedAsync()
		{
			var post = new Dictionary<string, string>()
			{
				{ "Email", "user@email.com" },
				{ "Password", "123456" }
			};
			string loginPage = "login";
			string expectedRedirectedPage = "dashBoard";
			var selector = new HttpPost.CssSelectorForDom()
			{
				Form = "form.login-form"
			};

			var Poster = new HttpPost(ROOT_URI);

			//Act
			var response = await Poster.PostBackWithHtmlForm(loginPage, selector, post, expectedRedirectedPage);

			//Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal(response.RequestMessage.RequestUri.ToString().ToLower(), (ROOT_URI + '/'+ expectedRedirectedPage).ToLower());
		}

		[Fact]
		public async void PostInputs_Values_JsonSuccess()
		{
			var Poster = await Login();
			var post = new Dictionary<string, string>()
			{
				{ "Food", "adsdcxsd" },
				{ "Search", "assd" }
			};
			string targetPage = "Food/Search";

			//Act
			var result = await Poster.Post(targetPage, post);


			//Assert
			var statusCode = result.StatusCode;
			string html = await result.Content.ReadAsStringAsync();
			var json = html.ToJson();

			Assert.Equal(json["IsSuccess"].ToString(), "False");
			Assert.Equal(HttpStatusCode.OK, statusCode);
		}
	}
}
