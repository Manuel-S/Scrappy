using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scrappy;

namespace ScrappyTest
{
	[TestClass]
	public class FormTests
	{
		[TestMethod]
		public void SubmitShouldCaptureAllFormFields()
		{
			var handler = new FakeHandler();
			var client = new HttpClient(handler);
			var browser = new Browser(client);


			handler.RequestHandlers.Add((request) => new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(File.ReadAllText("simple form.html"))
			});
			var page = browser.Open("http://google.com/testpage.html").Result;

			var form = page.GetForm("form");


			handler.RequestHandlers.Add((request) =>
			{
				var requestcontent = request.Content.ReadAsStringAsync().Result;
				Assert.AreEqual("id=50&name=&email=&betreff=Hilfe&text=&submit=absenden", requestcontent);
				return new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(File.ReadAllText("simple form.html"))
				};
			});

			var waitForRequestToFinish = form.Submit().Result;

		}
	}
}
