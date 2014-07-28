using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scrappy;
using ScrappyTest.Helpers;

namespace ScrappyTest
{
    [TestClass]
    [DeploymentItem("Content/simple form.html")]
    public class FormTests
    {
        [TestMethod]
        public void SubmitShouldCaptureAllFormFields()
        {
            var fakeHandler = new FakeHandler();
            var client = new HttpClient(fakeHandler);
            var browser = new Browser(client);


            fakeHandler.RequestHandlers.Add((request) => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText("Content/simple form.html"))
            });
            var page = browser.Open("http://google.com/testpage.html").Result;

            var form = page.GetForm("form");


            fakeHandler.RequestHandlers.Add((request) =>
            {
                var requestcontent = request.Content.ReadAsStringAsync().Result;

                requestcontent.Is("id=50&name=&email=&betreff=Hilfe&text=&submit=absenden");

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Everything worked if this is returned")
                };
            });

            var waitForRequestToFinish = form.Submit().Result;

            waitForRequestToFinish.ToString().Is("Everything worked if this is returned");

        }
    }
}
