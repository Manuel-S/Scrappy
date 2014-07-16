using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CsQuery;

namespace Scrappy
{
	public enum HttpVerb
	{
		Get,
		Post
	}

	public class Browser
	{
		private readonly HttpClient client;

		public Browser()
		{
			DocType = DocType.Default;
			ParsingOptions = HtmlParsingOptions.Default;
			ParsingMode = HtmlParsingMode.Auto;
			client = new HttpClient();
		}

		public Browser(HttpClient client)
		{
			DocType = DocType.Default;
			ParsingOptions = HtmlParsingOptions.Default;
			ParsingMode = HtmlParsingMode.Auto;
			this.client = client;
		}

		public HtmlParsingMode ParsingMode { get; set; }

		public HtmlParsingOptions ParsingOptions { get; set; }

		public DocType DocType { get; set; }

		public async Task<WebPage> Open(string url)
		{
			var uri = new Uri(url);
			var content = await client.GetStringAsync(uri);
			return new WebPage(this, content, new Uri(uri.GetLeftPart(UriPartial.Path)));
		}



		public async Task<WebPage> SendFormData(string url, HttpVerb method, Dictionary<string, string> formData, bool asJson)
		{
			//TODO: if get request make a querystring instead of json (MS)
			var httpcontent = new StringContent(asJson ? formData.ToJson() : formData.ToQuery());

			return await SendFormData(url, method, httpcontent);
		}

		public async Task<WebPage> SendFormData(string url, HttpVerb method, HttpContent httpcontent)
		{
			var uri = new Uri(url);
			string content;

			switch (method)
			{
				case HttpVerb.Post:
					var response = await client.PostAsync(uri, httpcontent);
					response.EnsureSuccessStatusCode();
					content = await response.Content.ReadAsStringAsync();
					break;
				case HttpVerb.Get:
					throw new NotImplementedException();
				default:
					throw new ArgumentException("Invalid HTTP Verb");
			}

			return new WebPage(this, content, new Uri(uri.GetLeftPart(UriPartial.Path)));
		}
	}
}
