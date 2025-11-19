using KCRM.Data;
using KCRM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;


namespace KCRM.Controllers
{
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
                .Where(n => n.IsDeleted == 0)
                .ToListAsync();

            return View(notes);
        }

        // GET: Notes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.IsDeleted == 0);

            if (note == null) return NotFound();

            return View(note);
        }

        // GET: Notes/Add
        public IActionResult Add()
        {
            return View();
        }

        // POST: Notes/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Content,UserId")] Notes note)
        {
            if (!ModelState.IsValid) return View(note);

            note.IsDeleted = 0;
            _context.Add(note);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Notes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var note = await _context.Notes.FindAsync(id);
            if (note == null || note.IsDeleted == 1) return NotFound();

            return View(note);
        }

        // POST: Notes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,UserId")] Notes incoming)
        {
            if (id != incoming.Id) return NotFound();

            if (!ModelState.IsValid) return View(incoming);

            var existing = await _context.Notes.FindAsync(id);
            if (existing == null || existing.IsDeleted == 1) return NotFound();

            // Sadece izin verilen alanları güncelle
            existing.Content = incoming.Content;
            existing.UserId = incoming.UserId;

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

        // GET: Notes/Delete/5 (soft delete confirmation)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.IsDeleted == 0);

            if (note == null) return NotFound();

            return View(note);
        }

        // POST: Notes/Delete/5 (soft delete)
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
