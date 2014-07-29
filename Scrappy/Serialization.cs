using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Scrappy
{
    internal static class Serialization
    {

        internal static string ToJson<T>(this T oObject)
        {
            return JsonConvert.SerializeObject(
                oObject,
                Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }

        public static string ToQuery<T>(this T obj)
        {
            var properties =
                obj.GetType()
                    .GetProperties()
                    .Where(p => p.GetValue(obj, null) != null)
                    .Select(p => p.Name + "=" + Uri.EscapeDataString(p.GetValue(obj, null).ToString()));


            return String.Join("&", properties.ToArray());
        }

        public static string ToQuery(this Dictionary<string, string> obj)
        {
            var properties =
                obj.Select(p => p.Key + "=" + Uri.EscapeDataString(p.Value ?? string.Empty));


            return String.Join("&", properties.ToArray());
        }
    }
}
