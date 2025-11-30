using KCRM.Data;
using KCRM.Models;
using KCRM.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace KCRM.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("", "Kullanıcı adı zaten mevcut");
                    return View(model);
                }
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Bu e-posta zaten kayıtlı");
                    return View(model);
                }

                CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Role = "User"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }

            return View(model);
        }


        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre yanlış");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)

            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                          new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // Şifreyi hash'leme metodu
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(storedHash);
        }
        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            // 1. Giriş yapan kullanıcının ID'sini al
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // 2. Kullanıcıyı bul
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // 3. İstatistikleri Hesapla (Sadece BU kullanıcıya ait veriler)
            int noteCount = await _context.Notes
                .CountAsync(n => n.UserId == userId && n.IsDeleted == 0);

            int taskCount = await _context.Tasks // Veya _context.Tasks
                .CountAsync(t => t.UserId == userId && t.IsDeleted == 0);

            int pendingTaskCount = await _context.Tasks
                .CountAsync(t => t.UserId == userId && t.Status == Models.TaskStatus.Bekliyor && t.IsDeleted == 0);

            // 4. Son Görevleri Çek (Örn: Son 5 görev)
            var recentTasks = await _context.Tasks // Veya _context.Tasks
                .Where(t => t.UserId == userId && t.IsDeleted == 0)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .Include(t => t.Customer) // Müşteri ismini göstermek için
                .Include(t => t.Lead) // Lead verilerini çekmek için
                .ToListAsync();

            // 5. ViewModel'i Doldur
            var model = new UserProfileViewModel
            {
                User = user,
                Role = userRole,
                TotalNotesCount = noteCount,
                TotalTasksCount = taskCount,
                PendingTasksCount = pendingTaskCount,
                RecentTasks = recentTasks
            };

            return View(model);
        }
    }
}
