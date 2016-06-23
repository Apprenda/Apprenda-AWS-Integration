using IgniteUICSMVC5Razor.Models;
using IgniteUICSMVC5Razor.ViewModels;
using System.Web.Mvc;

namespace IgniteUICSMVC5Razor.Controllers
{
    public class ExamplesController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult JavaScript()
        {
            return View();
        }

        [ActionName("mvc-helper")]
        public ActionResult MvcHelper()
        {
            ExampleViewModel vm = new ExampleViewModel();

            CustomerRepository repository = new CustomerRepository();

            vm.Customers = repository.GetAll();

            vm.CustomerCountSummaries = repository.GetCounts();

            return View(vm);
        }

    }
}
