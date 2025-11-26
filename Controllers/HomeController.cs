using System.Diagnostics;
using KCRM.Models;
using KCRM.Data;
using KCRM.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KCRM.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        // Logger ve DbContext inject
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            // Müþterileri ve görevlerini çek
            var customers = await _context.Customers
                .Include(c => c.Tasks)
                .ToListAsync();

            // ViewModel oluþtur
            var model = new DashboardViewModel
            {
                Customers = customers,
                TotalCustomers = customers.Count,
                TotalTasks = customers.Sum(c => c.Tasks?.Count ?? 0)
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied(string returnUrl)
        {
            TempData["AccessDenied"] = "Bu iþlemi yapma yetkiniz bulunmuyor!";
            return RedirectToAction("Index", "Home");
        }
    }
}
