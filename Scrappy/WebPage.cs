using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsQuery;

namespace Scrappy
{
    public class WebPage : CQ
    {
        private readonly Browser browser;
        private readonly Uri uri;

        internal WebPage(Browser b, string html, Uri uri)
            : base(html, b.ParsingMode, b.ParsingOptions, b.DocType)
        {
            browser = b;
            this.uri = uri;
            ResourceDictionary = new Dictionary<IDomObject, WebResource>();

            Select("img,script,style,link,iframe").Each(item =>
            {
                //img & script & iframe(not recursive yet)
                if (item.HasAttribute("src"))
                {
                    AddWebResource(item.GetAttribute("src"), item);
                }
                //stylesheet
                else if (item.HasAttribute("rel"))
                {
                    AddWebResource(item.GetAttribute("href"), item);
                }
            });


        }

        private void AddWebResource(string resourceUrl, IDomObject item)
        {
            var resource = new WebResource(ConstructUri(resourceUrl), browser, this, item.NodeName);

            ResourceDictionary.Add(item, resource);
        }

        public Form GetForm(string selector)
        {
            return GetForm(Select(selector));
        }

        public IEnumerable<WebResource> Resources { get { return ResourceDictionary.Select(x => x.Value); } }

        public Form GetForm(CQ nodes)
        {
            return new Form(browser, nodes, new Uri(uri.GetLeftPart(UriPartial.Path)));
        }

        private Dictionary<IDomObject, WebResource> ResourceDictionary { get; set; }

        public Task<WebPage> Follow(string selector)
        {
            var links = Select(selector);
            return Follow(links);
        }
        public Task<WebPage> Follow(CQ node)
        {
            if (node.Length == 0)
                throw new ArgumentException("No nodes found");
            if (!node.Is("a"))
                throw new ArgumentException("Node is not a link");

            var url = node.Attr("href");
            return Open(url);
        }

        public Task<WebPage> Open(string url)
        {
            var newuri = ConstructUri(url);
            return browser.Open(newuri.ToString());
        }

        private Uri ConstructUri(string url)
        {
            return new Uri(new Uri(uri.GetLeftPart(UriPartial.Path)), url);
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


        public override string ToString()
        {
            return base.SelectionHtml(true);
        }
    }
}
