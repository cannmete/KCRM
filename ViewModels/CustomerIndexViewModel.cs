using KCRM.Models;
using System.Collections.Generic;

namespace KCRM.ViewModels
{
    public class CustomerIndexViewModel
    {
        public List<Lead> Leads { get; set; } = new List<Lead>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
    }
}
