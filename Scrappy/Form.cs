using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsQuery;
using CsQuery.ExtensionMethods.Internal;

namespace Scrappy
{
	public class Form
	{
		private readonly Browser browser;
		private readonly CQ formNode;
		private readonly Uri baseUri;
		private readonly Dictionary<string, string> formValues;

		internal Form(Browser browser, CQ formNode, Uri baseUri)
		{
			this.browser = browser;
			this.formNode = formNode;
			this.baseUri = baseUri;

			if (!formNode.Is("form"))
			{
				throw new ArgumentException("Selected node must be of type <form />");
			}

			formValues = new Dictionary<string, string>();


			formValues.AddRange(
				formNode
				.Select("input[type=\"text\"]," +
						  "input[type=\"hidden\"]," +
						  "input[type=\"submit\"]," +
						  "input[type=\"radio\"]:checked," +
						  "input[type=\"checkbox\"]:checked," +
				        "select," +
				        "input:not([type])," +
				        "textarea")
				.Where(x => !string.IsNullOrWhiteSpace(x.Name))
				.Select(x =>
					new KeyValuePair<string, string>(x.Name, x.NodeName == "TEXTAREA" ? x.InnerHTML : x.Value)
					));
		}

		public Form Set(object values)
		{
			foreach (var propertyInfo in values.GetType().GetProperties())
			{
				formValues[propertyInfo.Name] = propertyInfo.GetValue(values).ToString();
			}
			return this;
		}

		public Form Set(string inputName, string value)
		{
			formValues[inputName]= value;
			return this;
		}

		public string this[string key]
		{
			get { return formValues[key]; }
			set { formValues[key] = value; }
		}


		public Task<WebPage> Submit(bool asJson = false)
		{

			var method = String.Compare("post", formNode.Attr("method"), StringComparison.OrdinalIgnoreCase) == 0
				? HttpVerb.Post : HttpVerb.Get;

			var formurl = formNode.Attr("action") ?? "";
			var uri = new Uri(baseUri, formurl);

			return browser.SendFormData(uri.ToString(), method, formValues, asJson);
		}
	}
}
