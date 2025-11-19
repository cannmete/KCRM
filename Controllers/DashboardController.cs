using KCRM.Data;
using KCRM.Models;
using KCRM.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KCRM.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Toplam Müşteri
            var totalCustomers = await _context.Customers.CountAsync(c => c.IsDeleted == 0);

            // Toplam Görev
            var totalTasks = await _context.Tasks.CountAsync(t => t.IsDeleted == 0);

            // Toplam Not
            var totalNotes = await _context.Notes.CountAsync(n => n.IsDeleted == 0);

            // Müşteri listesi
            var customers = await _context.Customers
                .Include(c => c.Tasks) // Müşteriye bağlı görevleri de yükle
                .Where(c => c.IsDeleted == 0)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                TotalCustomers = await _context.Customers.CountAsync(c => c.IsDeleted == 0),
                TotalTasks = await _context.Tasks.CountAsync(t => t.IsDeleted == 0),
                TotalNotes = await _context.Notes.CountAsync(n => n.IsDeleted == 0),
                Customers = await _context.Customers
        .Include(c => c.Tasks)
        .Where(c => c.IsDeleted == 0)
        .ToListAsync()
            };

            return View(model);
        }
    }
}
