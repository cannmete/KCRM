using KCRM.Data;
using KCRM.Models;
using KCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KCRM.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tasks (Listeleme)
        public async Task<IActionResult> Index()
        {
            var tasks = await _context.Tasks
                .Include(t => t.User)      // Görevi atayan
                .Include(t => t.Customer)  // Bağlı Müşteri
                .Include(t => t.Lead)      // Bağlı Aday
                .Where(t => t.IsDeleted == 0)
                .OrderByDescending(t => t.CreatedAt) // En yeniler başta
                .ToListAsync();

            return View(tasks);
        }

        // GET: Tasks/Add
        public async Task<IActionResult> Add()
        {
            var model = new TaskAddViewModel
            {
                // Müşteri Listesi
                CustomerList = await _context.Customers
                    .Where(c => c.IsDeleted == 0)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.FullName })
                    .ToListAsync(),

                // Aday (Lead) Listesi
                LeadList = await _context.Leads
                    .Where(l => l.IsDeleted == 0)
                    .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.FullName })
                    .ToListAsync()
            };

            return View(model);
        }

        // POST: Tasks/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(TaskAddViewModel model)
        {
            // Validasyon Temizliği
            ModelState.Remove("Task.User");
            ModelState.Remove("Task.UserId");
            ModelState.Remove("Task.Customer");
            ModelState.Remove("Task.Lead");
            ModelState.Remove("CustomerList");
            ModelState.Remove("LeadList");

            if (!ModelState.IsValid)
            {
                // Hata varsa listeleri tekrar doldur
                model.CustomerList = await _context.Customers.Where(c => c.IsDeleted == 0).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.FullName }).ToListAsync();
                model.LeadList = await _context.Leads.Where(l => l.IsDeleted == 0).Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.FullName }).ToListAsync();
                return View(model);
            }

            var task = model.Task;

            // UserId'yi otomatik ata
            task.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            task.CreatedAt = DateTime.UtcNow;
            task.IsDeleted = 0;

            // MANTIK: Eğer kullanıcı "Lead" seçtiyse CustomerId'yi temizle, tam tersi de geçerli.
            if (task.LeadId != null) task.CustomerId = null;
            else if (task.CustomerId != null) task.LeadId = null;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks // Veya _context.TaskItems
                .Include(t => t.User)      // Görevi atayan
                .Include(t => t.Customer)  // Bağlı Müşteri
                .Include(t => t.Lead)      // Bağlı Lead
                .FirstOrDefaultAsync(m => m.Id == id);

            if (task == null) return NotFound();

            return View(task);
        }

            // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks
                .Include(t => t.Customer) // İsimlerini göstermek için yüklüyoruz
                .Include(t => t.Lead)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null || task.IsDeleted == 1) return NotFound();

            return View(task);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem task)
        {
            if (id != task.Id) return NotFound();

            // İlişkisel nesneler formdan gelmediği için validasyondan çıkarıyoruz
            ModelState.Remove("User");
            ModelState.Remove("Customer");
            ModelState.Remove("Lead");

            if (!ModelState.IsValid)
            {
                // Hata varsa view'e dönerken isimleri tekrar göstermek için yüklemeliyiz
                // Ancak pratiklik adına direkt dönüyoruz (isimler kaybolabilir ama form çalışır)
                return View(task);
            }

            try
            {
                // 1. Mevcut görevi veritabanından çek (Değişmemesi gereken alanları korumak için)
                var existingTask = await _context.Tasks.FindAsync(id);
                if (existingTask == null) return NotFound();

                // 2. Sadece değişebilen alanları güncelle
                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.Priority = task.Priority;
                existingTask.Status = task.Status;
                existingTask.DueDate = task.DueDate;

                // UserId, CreatedAt, CustomerId ve LeadId'ye DOKUNMUYORUZ.
                // Böylece kimin atadığı ve kime atandığı bilgisi asla bozulmuyor.

                _context.Update(existingTask);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Tasks.AnyAsync(e => e.Id == task.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }
    }
    
}