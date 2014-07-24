using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsQuery.ExtensionMethods;

namespace Scrappy
{
    public class WebResource
    {
        public Uri Uri { get; private set; }
        private readonly Browser browser;
        private readonly WebPage webPage;
        private readonly string nodeName;
        private byte[] content;
        private Task<byte[]> activeDownload;

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
            }

        }

        public string GuessMimeType { get; internal set; }

        public async Task<byte[]> ForceDownload()
        {
            var response = await browser.Client.GetAsync(Uri);

            GuessMimeType = response.Content.Headers.ContentType.ToString();

            content = await response.GetContentAsBytes();

            return content;
        }

        public Task<byte[]> Content
        {
            get
            {
                if (content != null)
                    return Task.FromResult(content);
                return activeDownload ?? (activeDownload = ForceDownload());
            }
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
