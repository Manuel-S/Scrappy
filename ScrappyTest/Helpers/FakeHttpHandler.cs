using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ScrappyTest.Helpers
{
    public delegate HttpResponseMessage RequestHandler(HttpRequestMessage currentRequest);

    public class FakeHandler : HttpMessageHandler
    {

        public FakeHandler()
        {
            Responses = new List<HttpResponseMessage>();
            Requests = new List<HttpRequestMessage>();
            RequestHandlers = new List<RequestHandler>();
        }

        public bool RepeatLastHandler { get; set; }

        public List<HttpResponseMessage> Responses { get; set; }
        public List<HttpRequestMessage> Requests { get; set; }

        public List<RequestHandler> RequestHandlers { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (!RequestHandlers.Any())
            {
                return Task.FromResult<HttpResponseMessage>(null);
            }

            var count = Responses.Count();

            if (RequestHandlers.Count() > count)
            {
                var response = RequestHandlers.Skip(count).First().Invoke(request);
                Responses.Add(response);
                return Task.FromResult(response);
            }

            if (RepeatLastHandler)
            {
                var response = RequestHandlers.Last().Invoke(request);
                Responses.Add(response);
                return Task.FromResult(response);
            }


            Assert.Fail("Not enough handlers defined for this test and RepeatLastHandler set to false.");
            return Task.FromResult<HttpResponseMessage>(null);
        }
    }
}
