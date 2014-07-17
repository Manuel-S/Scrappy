using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

			client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
			client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
			client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
			client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");


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
			var response = await client.GetAsync(uri);
			string content;
			if (response.Headers.Any(x => x.Key == "Content-Encoding" && x.Value.Contains("gzip")))
			{
				using (var decompressedStream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress))
				using (var streamReader = new StreamReader(decompressedStream))
				{
					content = await streamReader.ReadToEndAsync();
				}
			}
			else
			{
				content = await response.Content.ReadAsStringAsync();
			}
			return new WebPage(this, content, uri);
		}



		public async Task<WebPage> OpenWithData(string url, HttpVerb method, Dictionary<string, string> formData, bool asJson)
		{
			if (method == HttpVerb.Get)
			{
				var uri = new Uri(url);
				var key = !string.IsNullOrWhiteSpace(uri.Query) ? '&' : '?';

				var geturi = new Uri(uri, key + formData.ToQuery());

				return await Open(geturi.ToString());
			}


			var httpcontent = new StringContent(asJson ? formData.ToJson() : formData.ToQuery());

			return await OpenWithData(url, method, httpcontent);
		}

		public async Task<WebPage> OpenWithData(string url, HttpVerb method, HttpContent httpcontent)
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
