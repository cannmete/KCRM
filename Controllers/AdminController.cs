using KCRM.Data;
using KCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> UserList()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }


        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }


        [HttpPost]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Role = role;
            await _context.SaveChangesAsync();

            return RedirectToAction("UserList");
        }
        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Email,Role")] User user)
        {
            if (id != user.Id) return NotFound();

            // Validasyon sırasında şifre alanlarını (PasswordHash/Salt) kontrol etmemesi için kaldırıyoruz.
            // Çünkü bu formda şifre değiştirilmiyor.
            ModelState.Remove("PasswordHash");
            ModelState.Remove("PasswordSalt");
            ModelState.Remove("Tasks");
            ModelState.Remove("Notes");
            ModelState.Remove("Customers");

            if (!ModelState.IsValid) return View(user);

            try
            {
                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null) return NotFound();

                // Sadece izin verilen alanları güncelle
                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.Role = user.Role; // Admin buradan rolü de değiştirebilir

                // Şifre ve diğer ilişkisel veriler (Tasks, Notes) KORUNUR.

                _context.Update(existingUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Users.AnyAsync(u => u.Id == user.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(UserList));
        }
        // GET: Admin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // DİKKAT: Bu işlem kullanıcıyı ve ilişkili verilerini (Cascade varsa) tamamen siler.
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(UserList));
        }
    }
}
