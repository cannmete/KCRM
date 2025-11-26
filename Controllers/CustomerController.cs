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
            var leads = await _context.Customers
                .Include(c => c.Tasks)
                .Where(c => c.IsLead && c.IsDeleted == 0)
                .ToListAsync();

            var customers = await _context.Customers
                .Include(c => c.Tasks)
                .Where(c => !c.IsLead && c.IsDeleted == 0)
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
        public async Task<IActionResult> Add([Bind("FullName,Email,Phone,Address,UserId,IsLead")] Customer customer)
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

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null || customer.IsDeleted == 1) return NotFound();

            return View(customer);
        }

        // POST: Customer/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,Phone,Address,UserId")] Customer incoming)
        {
            if (id != incoming.Id) return NotFound();
            if (!ModelState.IsValid) return View(incoming);

            var existing = await _context.Customers.FindAsync(id);
            if (existing == null || existing.IsDeleted == 1) return NotFound();

            existing.FullName = incoming.FullName;
            existing.Email = incoming.Email;
            existing.Phone = incoming.Phone;
            existing.Address = incoming.Address;
            existing.UserId = incoming.UserId;

            _context.Update(existing);
            await _context.SaveChangesAsync();

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
