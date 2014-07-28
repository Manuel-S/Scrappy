using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scrappy;
using ScrappyTest.Helpers;

namespace ScrappyTest
{
    [TestClass]
    public class ResourceTests
    {
        [TestMethod]
        public void ShouldAutoDownloadResources()
        {
            var handler = new FileSystemHandler(false);
            var client = new HttpClient(handler);
            var browser = new Browser(client) { AutoDownloadResources = true };

            var page = browser.Open("http://example.com/simple_resources.html").Result;

            page.Resources.Count().Is(4);

            handler.Requests.Count.Is(5);
        }

        [TestMethod]
        public void ShouldNotAutoDownloadResources()
        {
            var handler = new FileSystemHandler(false);
            var client = new HttpClient(handler);
            var browser = new Browser(client) { AutoDownloadResources = false };

            var page = browser.Open("http://example.com/simple_resources.html").Result;

            page.Resources.Count().Is(4);

            handler.Requests.Count.Is(1);
        }

        [TestMethod]
        public void ShouldGuessMimeTypesWithoutDownloading()
        {
            var handler = new FileSystemHandler(false);
            var client = new HttpClient(handler);
            var browser = new Browser(client) { AutoDownloadResources = false };

            var page = browser.Open("http://example.com/simple_resources.html").Result;

            var resources = page.Resources.ToArray();

            resources.Length.Is(4);

            resources[0].GuessMimeType.Is("text/css");
            resources[1].GuessMimeType.Is("image/png");
            resources[2].GuessMimeType.Is("text/html");
            resources[3].GuessMimeType.Is("application/javascript");

            handler.Requests.Count.Is(1);
        }

        [TestMethod]
        public void ShouldTakeMimeTypeFromDownload()
        {
            var handler = new FakeHandler { RepeatLastHandler = true };
            var client = new HttpClient(handler);
            var browser = new Browser(client) { AutoDownloadResources = true };

            handler.RequestHandlers.Add((request) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText("Content/simple_resources.html"))
                };
                response.Content.Headers.ContentType.MediaType = "text/html";
                return response;
            });

            var page = browser.Open("http://example.com/simple_resources.html").Result;

            var resources = page.Resources.ToArray();

            resources.Length.Is(4);

            resources.Select(x => x.GuessMimeType.Split(';').First())
                .All(x => x == "text/html").IsTrue();


            handler.Requests.Count.Is(5);
        }

        [TestMethod]
        public void ShouldOnlyDownloadResourcesOnce()
        {
            var handler = new FileSystemHandler(false);
            var client = new HttpClient(handler);
            var browser = new Browser(client) { AutoDownloadResources = false };

            var page = browser.Open("http://example.com/simple_resources.html").Result;

            var css = page.Resources.First();

            handler.Requests.Count.Is(1);

            var bytes = css.ReadAsBytes().Result;

            var str = css.ReadAsString().Result;

            handler.Requests.Count.Is(2);
            str.Is(File.ReadAllText("Content/demo_style.css"));
        }
    }
}
