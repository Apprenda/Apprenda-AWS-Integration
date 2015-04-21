using IgniteUICSMVC5Razor.Models;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IgniteUIMvc4Razor.Controllers
{
    public class IGCustomersController : ApiController
    {
        public HttpResponseMessage Get()
        {
            CustomerRepository repository = new CustomerRepository();
            var all = repository.GetAll().ToList<IGCustomer>();

            return Request.CreateResponse(HttpStatusCode.OK, all, "application/json");
        }
    }
}
