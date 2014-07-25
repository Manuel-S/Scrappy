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
        internal HttpClient Client { get; set; }

        public Browser()
        {
            DocType = DocType.Default;
            ParsingOptions = HtmlParsingOptions.Default;
            ParsingMode = HtmlParsingMode.Auto;
            Client = new HttpClient();

            Client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");


        }

        public Browser(HttpClient client)
        {
            Client = client;
            DocType = DocType.Default;
            ParsingOptions = HtmlParsingOptions.Default;
            ParsingMode = HtmlParsingMode.Auto;
        }

        public HtmlParsingMode ParsingMode { get; set; }

        public HtmlParsingOptions ParsingOptions { get; set; }

        public DocType DocType { get; set; }

        public bool AutoDownloadResources { get; set; }

        public async Task<WebPage> Open(string url)
        {
            var uri = new Uri(url);
            var response = await Client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            var content = await response.GetContentAsString();
            var page = new WebPage(this, content, uri);

            if (AutoDownloadResources)
            {
                await Task.WhenAll(page.Resources.Select(res => res.Response));
            }

            return page;
        }



        public async Task<WebPage> OpenWithFormData(string url, HttpVerb method, Dictionary<string, string> formData, bool asJson)
        {
            if (method == HttpVerb.Get)
            {
                var uri = new Uri(url);
                var key = !string.IsNullOrWhiteSpace(uri.Query) ? '&' : '?';

                var geturi = new Uri(uri, key + formData.ToQuery());

                return await Open(geturi.ToString());
            }


            var httpcontent = new StringContent(asJson ? formData.ToJson() : formData.ToQuery());

            return await OpenWithFormData(url, method, httpcontent);
        }

        public async Task<WebPage> OpenWithFormData(string url, HttpVerb method, HttpContent httpcontent)
        {
            var uri = new Uri(url);
            string content;

            switch (method)
            {
                case HttpVerb.Post:
                    var response = await Client.PostAsync(uri, httpcontent);
                    response.EnsureSuccessStatusCode();
                    content = await response.Content.ReadAsStringAsync();
                    break;
                case HttpVerb.Get:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Invalid HTTP Verb");
            }

            var page = new WebPage(this, content, new Uri(uri.GetLeftPart(UriPartial.Path)));

            if (AutoDownloadResources)
            {
                await Task.WhenAll(page.Resources.Select(res => res.Response));
            }

            return page;
        }
    }
}
