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

        // POST: Leads/Convert/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertToCustomer(int id)
        {
            // 1. Transaction Başlat (Hata olursa yarım kalmasın diye)
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 2. Lead'i ilişkileriyle beraber çek (Tasks ve Notes önemli)
                var lead = await _context.Leads
                    .Include(l => l.Tasks)
                    .Include(l => l.Notes)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (lead == null || lead.IsDeleted == 1)
                {
                    return NotFound();
                }

                // 3. Yeni Müşteri Nesnesi Oluştur
                var newCustomer = new Customer
                {
                    FullName = lead.FullName,
                    Email = lead.Email,
                    Phone = lead.Phone,
                    // Customer'da Adres zorunlu, Lead'de boşsa placeholder koyuyoruz
                    Address = string.IsNullOrEmpty(lead.Address) ? "Adres girilmedi (Adaydan dönüştürüldü)" : lead.Address,
                    UserId = lead.UserId, // Aday kiminse müşteri de onun olsun
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = 0,
                    Notes = new List<Notes>() // Not listesini başlat
                };

                // 4. Müşteriyi Ekle ve Kaydet (ID oluşması için)
                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();

                // 5. Lead'e ait Görevleri Müşteriye Taşı
                if (lead.Tasks != null)
                {
                    foreach (var task in lead.Tasks)
                    {
                        task.CustomerId = newCustomer.Id; // Müşteriye bağla
                        task.LeadId = null;               // Lead bağlantısını kopar
                        _context.Entry(task).State = EntityState.Modified;
                    }
                }

                // 6. Lead'e ait Notları Müşteriye Taşı
                if (lead.Notes != null)
                {
                    foreach (var note in lead.Notes)
                    {
                        note.CustomerId = newCustomer.Id;
                        note.LeadId = null;
                        _context.Entry(note).State = EntityState.Modified;
                    }
                }

                // 7. Lead Bilgilerini (Şirket, Kaynak) Kaybetmemek için Not Olarak Ekle
                var conversionNoteContent = $"Bu müşteri Aday listesinden dönüştürüldü.\n" +
                                            $"Eski Şirket Adı: {lead.CompanyName ?? "-"}\n" +
                                            $"Kaynak: {lead.Source ?? "-"}";

                var infoNote = new Notes
                {
                    Content = conversionNoteContent,
                    CustomerId = newCustomer.Id,
                    UserId = lead.UserId, // İşlemi yapan veya sahibi
                    IsDeleted = 0
                };
                _context.Notes.Add(infoNote);

                // 8. Lead'i Silinmiş İşaretle (Soft Delete)
                lead.IsDeleted = 1;
                _context.Leads.Update(lead);

                // Tüm değişiklikleri veritabanına uygula
                await _context.SaveChangesAsync();

                // İşlemi onayla
                await transaction.CommitAsync();

                // Başarılı olunca yeni müşterinin detayına git
                return RedirectToAction("Details", "Customer", new { id = newCustomer.Id });
            }
            catch (Exception ex)
            {
                // Hata olursa her şeyi geri al
                await transaction.RollbackAsync();
                // Hatayı loglayabilir veya kullanıcıya gösterebilirsin
                return BadRequest("Dönüştürme işlemi sırasında bir hata oluştu: " + ex.Message);
            }
        }
    }
}