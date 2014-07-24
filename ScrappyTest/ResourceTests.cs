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

            var page = browser.Open("http://example.com/simple resources.html").Result;

            Assert.AreEqual(4, page.Resources.Count(), "Resources");

            Assert.AreEqual(5, handler.Requests.Count, "Total Requests");
        }

        [TestMethod]
        public void ShouldNotAutoDownloadResources()
        {
            var handler = new FileSystemHandler(false);
            var client = new HttpClient(handler);
            var browser = new Browser(client) { AutoDownloadResources = false };

            var page = browser.Open("http://example.com/simple resources.html").Result;

            Assert.AreEqual(4, page.Resources.Count(), "Resources");

            Assert.AreEqual(1, handler.Requests.Count, "Total Requests");
        }

        [TestMethod]
        public void ShouldGuessMimeTypesWithoutDownloading()
        {
            var handler = new FileSystemHandler(false);
            var client = new HttpClient(handler);
            var browser = new Browser(client) { AutoDownloadResources = false };

            var page = browser.Open("http://example.com/simple resources.html").Result;

            var resources = page.Resources.ToArray();

            Assert.AreEqual(4, resources.Length, "Resources");

            Assert.AreEqual("text/css", resources[0].GuessMimeType);
            Assert.AreEqual("image/png", resources[1].GuessMimeType);
            Assert.AreEqual("text/html", resources[2].GuessMimeType);
            Assert.AreEqual("application/javascript", resources[3].GuessMimeType);

            Assert.AreEqual(1, handler.Requests.Count, "Total Requests");
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
                    Content = new StringContent(File.ReadAllText("Content/simple resources.html"))
                };
                response.Content.Headers.ContentType.MediaType = "text/html";
                return response;
            });

            var page = browser.Open("http://example.com/simple resources.html").Result;

            var resources = page.Resources.ToArray();

            Assert.AreEqual(4, resources.Length, "Resources");

            // split off the charset information if there is any
            Assert.AreEqual("text/html", resources[0].GuessMimeType.Split(';').First());
            Assert.AreEqual("text/html", resources[1].GuessMimeType.Split(';').First());
            Assert.AreEqual("text/html", resources[2].GuessMimeType.Split(';').First());
            Assert.AreEqual("text/html", resources[3].GuessMimeType.Split(';').First());

            Assert.AreEqual(5, handler.Requests.Count, "Total Requests");
        }
    }
}
