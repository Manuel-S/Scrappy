using System;
using System.Globalization;
using System.Threading.Tasks;
using CsQuery;

namespace Scrappy
{
	public class WebPage : CQ
	{
		private readonly Browser browser;
		private readonly Uri uri;

		internal WebPage(Browser b, string html, Uri uri) : base(html, b.ParsingMode, b.ParsingOptions, b.DocType)
		{
			browser = b;
			this.uri = uri;
		}

		public Form GetForm(string selector)
		{
			return GetForm(Select(selector));
		}

		public Form GetForm(CQ nodes)
		{
			return new Form(browser, nodes, new Uri(uri.GetLeftPart(UriPartial.Path)));
		}

		public Task<WebPage> Follow(string selector)
		{
			var links = Select(selector);
			return Follow(links);
		}
		public Task<WebPage> Follow(CQ node)
		{
			if (node.Length == 0)
				throw new ArgumentException("No nodes found");
			if(!node.Is("a"))
				throw  new ArgumentException("Node is not a link");

			var url = node.Attr("href");
			return Open(url);
		}

		public Task<WebPage> Open(string url)
		{
			var newuri = new Uri(new Uri(uri.GetLeftPart(UriPartial.Path)), url);
			return browser.Open(newuri.ToString());
		}

		public Task<WebPage> Click(string text, bool partialMatch = true, CultureInfo culture = null, CompareOptions compareOptions = CompareOptions.IgnoreCase)
		{
			culture = culture ?? CultureInfo.CurrentCulture;

			var links = Select("a[href]");

			foreach (var link in links)
			{
				var follow = false;
				if (partialMatch)
				{
					var index = culture.CompareInfo.IndexOf(link.InnerText, text, compareOptions);
					if (index >= 0)
					{
						follow = true;
					}
				}
				else
				{
					var comparison = string.Compare(link.InnerText, text, culture, compareOptions);
					if (comparison == 0)
					{
						follow = true;
					}
				}
				if (follow)
				{
					var url = link.GetAttribute("href");
					return Open(url);
				}
			}

			throw new Exception("No matching link was found.");
		}

	}
}
