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


        public List<string> CustomerGraphLabels { get; set; } = new List<string>();
        public List<int> CustomerGraphValues { get; set; } = new List<int>();

        public List<string> TaskStatusLabels { get; set; } = new List<string>();
        public List<int> TaskStatusValues { get; set; } = new List<int>();
    }

}
