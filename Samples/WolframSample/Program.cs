﻿using System;
using System.Text.RegularExpressions;
using Scrappy;

namespace WolframSample
{
    class Program
    {
        private static void Main(string[] args)
        {
            // the browser holds cookies and stuff for us
            var browser = new Browser();

            //navigate to the landing page
            var startpage = browser.Open("http://www.wolframalpha.com/").Result;


            //submit our query using the Form helper
            var query = string.Join(" ", args);
            var resultpage = startpage.GetForm("#calculate").Set("i", query).Submit().Result;


            //get all <script>-Contents (the plain text result is hidden in the javascript)
            var page = resultpage.Select("script").SelectionHtml(true);


            //get the Box in which the result is displayed, if there is a result
            var resultindex = page.IndexOf(@"'\x22Result\x22'", StringComparison.InvariantCulture);
            if (resultindex <= 0)
            {
                //try to find a decimal approximation if there is no exact result available
                resultindex = page.IndexOf(@"'\x22DecimalApproximation\x22'", StringComparison.InvariantCulture);
            }
            var podindex = page.IndexOf("pod_", resultindex - 170, StringComparison.InvariantCulture);
            var podstr = page.Substring(podindex, 8);


            // on complex queries, the computation is done asynchronously and needs to be loaded seperately
            var isAsync = new Regex("asynchronousPod\\(\"(?<result>.*?)\", \".*?\", \".*?\","
                    + " \".*?\", \".*?\", \"Result\" \\)").Match(page);
            if (isAsync.Success)
            {
                var asyncResult = resultpage.Open(isAsync.Groups["result"].Value).Result;

                //replace the content we are going to look for the result with the asynchronously gathered content
                page = asyncResult.ToString();
            }


            // try to get the result string(s)
            var result = new Regex(podstr + ".*?stringified\": \"(?<result>.*?)\"")
                .Matches(page);


            foreach (Match match in result)
            {
                Console.WriteLine("Result:");
                Console.WriteLine(match.Groups["result"].Value.Replace("\\/", "/"));
            }

            if (result.Count == 0)
            {
                Console.WriteLine("No result found");
            }
        }
    }
}
