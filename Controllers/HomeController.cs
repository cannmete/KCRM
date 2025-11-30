using KCRM.Data;
using KCRM.Models;
using KCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace KCRM.Controllers
{
    [Authorize]
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
            // 1. ADIM: ÝSTATÝSTÝKLERÝ HESAPLA (VERÝTABANI TARAFLI)
            // Verileri RAM'e çekmeden, doðrudan veritabanýnda saydýrýyoruz (CountAsync).
            // Bu yöntem çok daha hýzlýdýr ve performans dostudur.

            // Silinmemiþ (Aktif) Müþteri Sayýsý
            int totalCustomers = await _context.Customers
                .CountAsync(c => c.IsDeleted == 0);

            // Silinmemiþ (Aktif) Görev Sayýsý
            int totalTasks = await _context.Tasks
                .CountAsync(t => t.IsDeleted == 0);

            // >>> YENÝ EKLENEN: Silinmemiþ (Aktif) Not Sayýsý <<<
            int totalNotes = await _context.Notes
                .CountAsync(n => n.IsDeleted == 0);


            // 2. ADIM: TABLO ÝÇÝN LÝSTEYÝ ÇEK
            // Dashboard'da tüm binlerce müþteriyi göstermek yerine, 
            // genellikle "Son Eklenen 10 Müþteri" veya "Tüm Müþteriler" listelenir.
            // Burada sizin yapýnýzý koruyarak listeyi çekiyoruz ama "IsDeleted" filtresi ekliyoruz.

            var activeCustomers = await _context.Customers
                .Include(c => c.Tasks) // Görev sayýsýný tabloda göstermek için Include þart
                .Where(c => c.IsDeleted == 0) // Sadece silinmemiþleri getir
                .OrderByDescending(c => c.CreatedAt) // (Opsiyonel) En yeniler üstte görünsün
                .ToListAsync();


            // 3. ADIM: VIEWMODEL OLUÞTUR
            var model = new DashboardViewModel
            {
                // Tabloda gösterilecek liste
                Customers = activeCustomers,

                // Hesaplanan Sayaçlar
                TotalCustomers = totalCustomers,
                TotalTasks = totalTasks,

                // >>> YENÝ EKLENEN NOT SAYISI <<<
                TotalNotes = totalNotes
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
