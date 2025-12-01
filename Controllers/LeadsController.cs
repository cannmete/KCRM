using KCRM.Data;
using KCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KCRM.Controllers
{
    [Authorize] 
    public class LeadsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeadsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Leads (Listeleme)
        public async Task<IActionResult> Index()
        {
            var leads = await _context.Leads
                .Where(l => l.IsDeleted == 0)
                .OrderByDescending(l => l.CreatedAt) // En yeniler üstte
                .ToListAsync();

            return View(leads);
        }

        // GET: Leads/Add (Ekleme Formu)
        public IActionResult Add()
        {
            return View();
        }

        // POST: Leads/Add (Kaydetme İşlemi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Lead lead)
        {
            // UserId'yi otomatik ata (Lead'i kim ekledi?)
            // Not: User nesnesi formdan gelmediği için validasyondan çıkarıyoruz.
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (!ModelState.IsValid) return View(lead);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            lead.UserId = userId;
            lead.IsDeleted = 0;
            lead.CreatedAt = DateTime.UtcNow;

            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Leads/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lead = await _context.Leads.FindAsync(id);
            if (lead == null || lead.IsDeleted == 1) return NotFound();

            return View(lead);
        }

        // POST: Leads/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,Phone,Address,CompanyName,Source")] Lead lead)
        {
            if (id != lead.Id) return NotFound();

            // UserId ve CreatedAt formdan gelmez, validasyondan çıkarıyoruz.
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (!ModelState.IsValid) return View(lead);

            try
            {
                // 1. Mevcut veriyi çek
                var existing = await _context.Leads.FindAsync(id);
                if (existing == null || existing.IsDeleted == 1) return NotFound();

                // 2. Alanları güncelle (UserId ve CreatedAt KORUNUR)
                existing.FullName = lead.FullName;
                existing.Email = lead.Email;
                existing.Phone = lead.Phone;
                existing.Address = lead.Address;

                // Lead'e özel alanlar:
                existing.CompanyName = lead.CompanyName;
                existing.Source = lead.Source;

                // 3. Kaydet
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Leads.AnyAsync(e => e.Id == lead.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}