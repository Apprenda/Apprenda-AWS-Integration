using IgniteUICSMVC5Razor.Models;
using System.Collections.Generic;

namespace IgniteUICSMVC5Razor.ViewModels
{
    public class ExampleViewModel
    {
        public IEnumerable<IGCustomer> Customers { get; set; }

        public IEnumerable<IGCustomerCountSummary> CustomerCountSummaries { get; set; }

        //http://bit.ly/1e2g8Zy
        public Dictionary<string, object> AddAttribute(string attribute, string value)
        {
            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add(attribute, value);

            return attributes;
        }
    }
}
