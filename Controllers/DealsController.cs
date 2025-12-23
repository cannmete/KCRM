using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KCRM.Data;
using KCRM.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace KCRM.Controllers
{
    [Authorize]
    public class DealsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DealsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Deals
        public async Task<IActionResult> Index()
        {
            // Sadece silinmemiş fırsatları getir
            // Müşteri (Customer) ve Satışçı (User) bilgilerini de dahil et (Include)
            var applicationDbContext = _context.Deals
                .Include(d => d.Customer)
                .Include(d => d.User)
                .Where(d => d.IsDeleted == 0);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Deals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var deal = await _context.Deals
                .Include(d => d.Customer)
                .ThenInclude(c => c.Notes)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsDeleted == 0);

            if (deal == null) return NotFound();

            return View(deal);
        }

        // GET: Deals/Create
        public IActionResult Create()
        {
            // Müşteri seçimi için Dropdown listesi (Sadece silinmemişler)
            ViewData["CustomerId"] = new SelectList(_context.Customers.Where(c => c.IsDeleted == 0), "Id", "FullName");
            return View();
        }

        // POST: Deals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Amount,Stage,ClosingDate,CustomerId")] Deal deal)
        {
            // Giriş yapan kullanıcıyı otomatik ata
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                deal.UserId = userId;
            }

            // Eğer ModelState valid değilse bile (User ilişkisi bazen null görünebilir) manuel kontrol edelim
            if (deal.CustomerId > 0 && !string.IsNullOrEmpty(deal.Title))
            {
                deal.CreatedAt = DateTime.UtcNow;
                deal.IsDeleted = 0;

                _context.Add(deal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata varsa formu tekrar doldur
            ViewData["CustomerId"] = new SelectList(_context.Customers.Where(c => c.IsDeleted == 0), "Id", "FullName", deal.CustomerId);
            return View(deal);
        }

        // GET: Deals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var deal = await _context.Deals.FindAsync(id);
            if (deal == null || deal.IsDeleted == 1) return NotFound();

            ViewData["CustomerId"] = new SelectList(_context.Customers.Where(c => c.IsDeleted == 0), "Id", "FullName", deal.CustomerId);
            return View(deal);
        }

        // POST: Deals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Amount,Stage,ClosingDate,CustomerId,UserId,CreatedAt")] Deal deal)
        {
            if (id != deal.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DealExists(deal.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers.Where(c => c.IsDeleted == 0), "Id", "FullName", deal.CustomerId);
            return View(deal);
        }

        // GET: Deals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var deal = await _context.Deals
                .Include(d => d.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (deal == null) return NotFound();

            return View(deal);
        }

        // POST: Deals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal != null)
            {
                // Soft Delete (Veritabanından silmek yerine çöp kutusuna atıyoruz)
                deal.IsDeleted = 1;
                _context.Deals.Update(deal);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DealExists(int id)
        {
            return _context.Deals.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStage(int id, DealStage stage)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null)
            {
                return NotFound();
            }

            // Aşamayı güncelle
            deal.Stage = stage;

            // Eğer Kazanıldı veya Kaybedildi ise Kapanış Tarihini bugüne eşitle
            if (stage == DealStage.Won || stage == DealStage.Lost)
            {
                deal.ClosingDate = DateTime.Now;
            }

            _context.Update(deal);
            await _context.SaveChangesAsync();

            // Detay sayfasına geri dön
            return RedirectToAction(nameof(Details), new { id = deal.Id });
        }
    }
}   