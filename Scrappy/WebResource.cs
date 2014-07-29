using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsQuery.ExtensionMethods;
using System.Net;
using System.Net.Http;

namespace Scrappy
{
    public class WebResource
    {
        public Uri Uri { get; private set; }
        private readonly Browser browser;
        private readonly WebPage webPage;
        private readonly string nodeName;

        private static Dictionary<string, string> MimeTypesByExtension { get; set; }

        static WebResource()
        {
            MimeTypesByExtension = new Dictionary<string, string>();
            Resources.MimeTypesByExtension.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Split('\t'))
                .Where(x => x[0].Length > 0)
                .ForEach(x => MimeTypesByExtension[x[0]] = x[1]);
        }

        internal WebResource(Uri uri, Browser browser, WebPage webPage, string nodeName)
        {
            this.Uri = uri;
            this.browser = browser;
            this.webPage = webPage;
            this.nodeName = nodeName;

            var path = uri.GetLeftPart(UriPartial.Path);
            if (Path.HasExtension(path))
            {
                var extension = Path.GetExtension(path);
                string guessMimeType;
                if (MimeTypesByExtension.TryGetValue(extension, out guessMimeType))
                {
                    GuessMimeType = guessMimeType;
                }
                else
                {
                    GuessMimeType = Resources.MimeTypesByExtension;
                }
            }

        }

        public string GuessMimeType { get; internal set; }

        private Task<HttpResponseMessage> responseTask;

        internal Task<HttpResponseMessage> Response
        {
            get
            {
                return responseTask ?? 
                    (responseTask = browser.Client.GetAsync(Uri)
                                    .ContinueWith((response) => { GuessMimeType = response.Result.Content.Headers.ContentType.MediaType;
                                    return response.Result;
                                    }));
            }
        }

        public async Task<byte[]> ReadAsBytes()
        {
            return await (await Response).GetContentAsBytes();
        }

        public async Task<string> ReadAsString()
        {
            return await (await Response).GetContentAsString();
        }

        public string NameSuggestion
        {
            get
            {
                if (Uri.PathAndQuery.Length > 5 && Uri.PathAndQuery.IndexOf('&') > 5)
                {
                    return Uri.GetLeftPart(UriPartial.Path).Substring(Uri.GetLeftPart(UriPartial.Path).LastIndexOf('/') + 1);
                }
                else if (Uri.OriginalString.Length < 5)
                {
                    return "index";
                }
                else
                {
                    return Uri.Host;
                }
            }
        }

    }
}
