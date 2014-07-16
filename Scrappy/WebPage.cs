using System;
using System.Globalization;
using System.Threading.Tasks;
using CsQuery;

namespace Scrappy
{
	public class WebPage : CQ
	{
		private readonly Browser browser;
		private readonly Uri baseUri;

		internal WebPage(Browser b, string html, Uri baseUri) : base(html, b.ParsingMode, b.ParsingOptions, b.DocType)
		{
			browser = b;
			this.baseUri = baseUri;
		}

		public Form GetForm(string selector)
		{
			return GetForm(Select(selector));
		}

		public Form GetForm(CQ nodes)
		{
			return new Form(browser, nodes, baseUri);
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
			var uri = new Uri(baseUri, url);
			return browser.Open(uri.ToString());
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
					var uri = new Uri(baseUri, url);
					return browser.Open(uri.ToString());
				}
			}

			throw new Exception("No matching link was found.");
		}
	}
}
