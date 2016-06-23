using IgniteUICSMVC5Razor.Models;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IgniteUIMvc4Razor.Controllers.api
{
    public class IGCustomerCountSummariesController : ApiController
    {
        public HttpResponseMessage Get()
        {
            CustomerRepository repository = new CustomerRepository();
            var counts = repository.GetCounts().ToList<IGCustomerCountSummary>();

            return Request.CreateResponse(HttpStatusCode.OK, counts, "application/json");
        }
    }
}
