using Newtonsoft.Json.Serialization;
using System.Web.Http;

namespace IgniteUICSMVC5Razor
{
    public class CustomGlobalConfiguration
    {
        public static void Customize(HttpConfiguration config)
        {
            // approach via @John_Papa at: http://jpapa.me/NqC2HH
            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
        }
    }
}
