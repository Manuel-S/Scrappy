


![Scrappy Logo](Scrappy - small.png)  Scrappy  
======= 


If you want to automate tasks in the web, this is your go to library. Navigate through websites with ease, fill out forms comfortably from your couch, let Scrappy do all the dirty work and get you what you want.

Features:

* Form-Handling
* Link-Handling
* Header-Mimicking
* HTML-Parsing and CSS-Selectors using [CsQuery][1]

####Usage####

How to fetch your IP from whatsmyip.org:
```csharp
var browser = new Browser();

var page = await browser.Open("http://www.whatsmyip.org/");

var myIp = page.Select("#ip").Text();

```


####Samples####

* [WolframCalculator][s1]: Commandline tool which sends an expression to wolframalpha.com and displays the plain-text result to the user.
* [Tests][s2]: Check out the tests for Scrappy to see what can be done with it. 


####Planned Features####

* Resource downloader and management
* Reactive extensions
* Javascript interpreter


####Continous Integration####

[![scrappy MyGet Build Status](https://www.myget.org/BuildSource/Badge/scrappy?identifier=5cb40dd1-496c-4a81-ae15-41162c8df6f5)](https://www.myget.org/)

Get the latest build from this NuGet feed: https://www.myget.org/F/scrappy/api/v2/

[1]: https://github.com/jamietre/CsQuery

[s1]: https://github.com/Manuel-S/Scrappy/blob/master/Samples/WolframSample/Program.cs
[s2]: https://github.com/Manuel-S/Scrappy/blob/master/ScrappyTest/
