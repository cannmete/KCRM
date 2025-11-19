using KCRM.Models;
using System.Collections.Generic;

namespace KCRM.ViewModels
{
    public class CustomerIndexViewModel
    {
        public List<Customer> Leads { get; set; } = new List<Customer>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
    }
}
