using KCRM.Models;

namespace KCRM.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalTasks { get; set; }
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public int TotalNotes { get; set; }
        public List<Notes> Notes { get; set; } = new List<Notes>();


    }

}
