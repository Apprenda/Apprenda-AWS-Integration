using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IgniteUICSMVC5Razor.Models
{
    public class IGCustomer
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName
        {
            get { return this.FirstName + " " + this.LastName; }
        }
    }
}
