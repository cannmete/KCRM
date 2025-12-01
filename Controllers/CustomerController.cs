using KCRM.Data;
using KCRM.Models;
using KCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KCRM.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer
        public async Task<IActionResult> Index()
        {
            var leads = await _context.Leads
                .Include(l => l.Tasks)
                .Where(l => l.IsDeleted == 0)
                .ToListAsync();

            var customers = await _context.Customers
                .Include(c => c.Tasks)
                .Where(c => c.IsDeleted == 0)
                .ToListAsync();

            var model = new CustomerIndexViewModel
            {
                Leads = leads,
                Customers = customers
            };

            return View(model);
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.Tasks)
                  .ThenInclude(t => t.User)
                .Include(c => c.Notes)
                  .ThenInclude(n => n.User)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == 0);

            if (customer == null) return NotFound();

            return View(customer);
        }


        // GET: Customer/Add
        [Authorize(Roles = "Admin")]
        public IActionResult Add()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        // POST: Customer/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("FullName,Email,Phone,Address,UserId")] Customer customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            // Giriş yapan kullanıcı ID'sini al
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            customer.UserId = userId;      // USERID BURADA AYARLANIYOR
            customer.IsDeleted = 0;
            customer.CreatedAt = DateTime.UtcNow;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Customer/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Sadece ID'ye göre çekiyoruz, ilişkileri (Include) getirmeye gerek yok çünkü düzenleme formunda sadece temel bilgiler var.
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null || customer.IsDeleted == 1) return NotFound();

            return View(customer);
        }

        // POST: Customer/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,Phone,Address")] Customer incoming)
        {
            // 1. ID Kontrolü
            if (id != incoming.Id) return NotFound();

            // 2. Validasyon Temizliği (User ve UserId formdan gelmediği için hata vermesini engelle)
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (!ModelState.IsValid) return View(incoming);

            try
            {
                // 3. Mevcut Veriyi Çek (Takip Edilen Entity)
                var existing = await _context.Customers.FindAsync(id);

                if (existing == null || existing.IsDeleted == 1) return NotFound();

                // 4. Sadece İzin Verilen Alanları Güncelle
                // UserId ve CreatedAt alanlarına DOKUNMUYORUZ. Böylece veri korunuyor.
                existing.FullName = incoming.FullName;
                existing.Email = incoming.Email;
                existing.Phone = incoming.Phone;
                existing.Address = incoming.Address;

                // Not: _context.Update(existing) demeye gerek yok, 
                // çünkü 'existing' veritabanından çekildiği için zaten takip ediliyor (Tracked).
                // SaveChanges yapınca değişiklikleri otomatik algılar.

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Customers.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Customer/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == 0);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customer/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            customer.IsDeleted = 1; // soft delete
            _context.Update(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
