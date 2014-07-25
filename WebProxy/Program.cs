using Microsoft.Owin.Hosting;
using Microsoft.Owin;
using Owin;
using Scrappy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsQuery;
using CsQuery.ExtensionMethods;

namespace WebProxy
{
    static class Program
    {
        static void Main(string[] args)
        {
            var options = new StartOptions
            {
                ServerFactory = "Nowin",
                Port = 5000
            };

            using (WebApp.Start<Startup>(options))
            {
                Console.WriteLine("Running a web proxy on port 5000");
                Console.ReadKey();
            }
        }
    }

    public class Startup
    {
        private static Browser browser = new Browser { AutoDownloadResources = false };

        private const string baseUrl = "http://localhost:5000/?q=";

        private string ConstructUrl(string pageUrl, WebPage page)
        {
            return baseUrl + Uri.EscapeDataString(page.ConstructUri(pageUrl).ToString());
        }

        private string TryUpdateUrls(string p, WebPage page)
        {
            //really naive

            p = p.RegexReplace("\"http://.*\";", (match) => ConstructUrl(match.Value, page));

            return p;
        }

        public void Configuration(IAppBuilder app)
        {
            app.Use((context, next) => {
                if (context.Request.Path.Value == "/" && context.Request.Query["q"] == null)
                {
                    context.Response.ContentType = "text/html";
                    return context.Response.WriteAsync(File.ReadAllText("Index.html"));
                }
                return next();
            });

            app.Run(async context =>
            {
                var q = context.Request.Query["q"];
                if (context.Request.Path.Value == "/" && q != null)
                { 
                    //do open browser page with q
                    WebPage page;
                    if (context.Request.Method == "POST")
                    {
                        var formdata = await context.Request.ReadFormAsync();
                        page = await browser.OpenWithFormData(q, HttpVerb.Post, formdata);
                    }
                    else if (context.Request.Method == "GET")
                    {
                        page = await browser.Open(q);
                    }
                    else
                    {
                        context.Response.StatusCode = 405;
                        context.Response.Headers.Append("Allow", "GET, POST");
                        return;
                    }

                    var mimeType = page.Response.Content.Headers.ContentType.MediaType;

                    context.Response.ContentType = mimeType;

                    if (mimeType.StartsWith("text"))
                    {
                        page.Select("img[src],script[src],link[rel='stylesheet']").Select(x => new CQ(x)).ForEach(elem => elem.Attr("src", ConstructUrl(elem.Attr("src"), page)));
                        page.Select("a[href]").Select(x => new CQ(x)).ForEach(elem => elem.Attr("href", ConstructUrl(elem.Attr("href"), page)));

                        page.Select("script").Select(x => new CQ(x)).ForEach(elem => elem.Html(TryUpdateUrls(elem.Html(), page)));
                        
                        
                        
                        await context.Response.WriteAsync(page.ToString());
                    }
                    else
                    {
                        var bytes = await page.Response.GetContentAsBytes();
                        await context.Response.WriteAsync(bytes);
                    }

                    

                }
                else
                {
                    context.Response.StatusCode = 404;
                }
            });
        }

    }



}
