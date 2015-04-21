using System.Collections.Generic;
using System.Linq;

namespace IgniteUICSMVC5Razor.Models
{
    public class CustomerRepository
    {
        public IEnumerable<IGCustomer> GetAll()
        {
            List<IGCustomer> customers = new List<IGCustomer>();
            customers.Add(new IGCustomer() { Id = 1, FirstName = "Zachary", LastName = "Roberts", IsActive = false });
            customers.Add(new IGCustomer() { Id = 2, FirstName = "Heidi", LastName = "Martin", IsActive = true });
            customers.Add(new IGCustomer() { Id = 3, FirstName = "Jen", LastName = "Wallace", IsActive = true });
            customers.Add(new IGCustomer() { Id = 4, FirstName = "Tyler", LastName = "Black", IsActive = false });
            customers.Add(new IGCustomer() { Id = 5, FirstName = "Aria", LastName = "Wellington", IsActive = false });

            return customers;
        }

        public IEnumerable<IGCustomerCountSummary> GetCounts()
        {
            var customers = this.GetAll();

            var activeQuery = from c in customers
                              where c.IsActive
                              select c;

            var inActiveQuery = from c in customers
                                where (c.IsActive == false)
                                select c;

            List<IGCustomerCountSummary> summaries = new List<IGCustomerCountSummary>();
            summaries.Add(new IGCustomerCountSummary { CategoryLabel = "Active", CustomerCount = activeQuery.Count() });
            summaries.Add(new IGCustomerCountSummary { CategoryLabel = "Inactive", CustomerCount = inActiveQuery.Count() });

            return summaries;
        }
    }
}
