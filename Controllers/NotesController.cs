using KCRM.Data;
using KCRM.Models;
using KCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace KCRM.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Notes
        public async Task<IActionResult> Index()
        {
            var notes = await _context.Notes
                .Include(n => n.User)
                .Include(n => n.Customer)
                .Where(n => n.IsDeleted == 0)
                .ToListAsync();

            return View(notes);
        }

        // GET: Notes/Details/5 (İyileştirildi: İlişkiler yüklendi)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var note = await _context.Notes
                .Include(n => n.User)
                .Include(n => n.Customer) // Customer bilgisini de yüklüyoruz
                .FirstOrDefaultAsync(n => n.Id == id && n.IsDeleted == 0);

            if (note == null) return NotFound();
            return View(note);
        }

        // GET: Notes/Add (ViewModel ile Müşteri Listesi Yüklendi)
        public async Task<IActionResult> Add()
        {
            // Yalnızca silinmemiş müşterileri listeye alıyoruz
            var customers = await _context.Customers
                .Where(c => c.IsDeleted == 0)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(), // Seçenek değeri (CustomerId)
                    Text = c.FullName        // Görünecek metin
                })
                .ToListAsync();

            var model = new NotesAddViewModel
            {
                CustomerList = customers
            };

            return View(model);
        }

        // POST: Notes/Add (ViewModel Kullanımı)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NotesAddViewModel model)
        {
            // >>> BU SATIRI EKLEYİN VE ÇIKTI PENCERESİNİ İZLEYİN <<<
            System.Diagnostics.Debug.WriteLine(">>> KONTROL EDİLİYOR: Gelen Müşteri ID: " + model.Note.CustomerId);
            ModelState.Remove("Note.User");
            ModelState.Remove("Note.UserId");
            // Customer nesnesi de formdan gelmez, sadece ID gelir.
            ModelState.Remove("Note.Customer");
            if (!ModelState.IsValid)
            {
                // Validasyon başarısız olursa, müşteri listesini tekrar yükleyip View'e dönmeliyiz.
                model.CustomerList = await _context.Customers
                    .Where(c => c.IsDeleted == 0)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.FullName })
                    .ToListAsync();
                return View(model);
            }

            // UserId'yi Otomatik Atama
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var note = model.Note;
            // DEBUG: Gelen CustomerId'yi Output penceresine yazdır
            System.Diagnostics.Debug.WriteLine("GELEN CUSTOMER ID: " + note.CustomerId);
            note.UserId = userId; // Otomatik atama bu aşamada oluyor.
            note.IsDeleted = 0;

            _context.Add(note);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Notes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Edit formu için User ve Customer ilişkileri yüklenmeli (View'de kullanılabilir)
            var note = await _context.Notes
                .Include(n => n.Customer)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (note == null || note.IsDeleted == 1) return NotFound();

            return View(note);
        }

        // POST: Notes/Edit/5 (Yetki Kontrolü ve Güvenlik İyileştirmesi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,UserId,CustomerId")] Notes incoming)
        {
            if (id != incoming.Id) return NotFound();

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            ModelState.Remove("User");
            ModelState.Remove("Customer");
            ModelState.Remove("Lead");

            if (!ModelState.IsValid) return View(incoming);

            var existing = await _context.Notes.FindAsync(id);
            if (existing == null || existing.IsDeleted == 1) return NotFound();

            // Yetki Kontrolü: Sadece notun sahibi veya Admin düzenleyebilir
            if (existing.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid(); // 403 Yetki Reddi
            }

            // Güncelleme
            existing.Content = incoming.Content;
            existing.CustomerId = incoming.CustomerId;

            try
            {
                _context.Update(existing);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.Notes.AnyAsync(n => n.Id == id);
                if (!exists) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Notes/Delete/5 (Silme Onayı - İlişkiler Yüklendi)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var note = await _context.Notes
                .Include(n => n.User)
                .Include(n => n.Customer) // Customer bilgisini de yüklüyoruz
                .FirstOrDefaultAsync(n => n.Id == id && n.IsDeleted == 0);

            if (note == null) return NotFound();
            return View(note);
        }

        // POST: Notes/Delete/5 (Soft Delete)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null) return NotFound();

            note.IsDeleted = 1;
            _context.Update(note);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}