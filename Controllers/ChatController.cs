using KCRM.Data;
using KCRM.Models; // Enum'lara erişmek için modelin namespace'ini eklemelisin
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly AiChatService _chatService;
    private readonly ApplicationDbContext _context;

    public ChatController(AiChatService chatService, ApplicationDbContext context)
    {
        _chatService = chatService;
        _context = context;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Mesaj boş olamaz.");

        // 1. Veritabanından verileri senin modeline göre filtreleyerek çekiyoruz
        // IsDeleted == 0 kuralını tüm sorgulara ekliyoruz ki silinmiş veriler gelmesin
        var customerCount = await _context.Customers.CountAsync(c => c.IsDeleted == 0);
        var leadCount = await _context.Leads.CountAsync(l => l.IsDeleted == 0);

        // Görevleri senin TaskStatus Enum yapına göre sayıyoruz
        var bekleyenGorevSayisi = await _context.Tasks
            .CountAsync(t => t.IsDeleted == 0 && t.Status == KCRM.Models.TaskStatus.Bekliyor);

        var islemdekiGorevSayisi = await _context.Tasks
            .CountAsync(t => t.IsDeleted == 0 && t.Status == KCRM.Models.TaskStatus.Islemde);

        // 2. Yapay zekaya göndereceğimiz zenginleştirilmiş bağlam (Context)
        string contextData = $"Güncel Veritabanı Özeti: Toplam {customerCount} kayıtlı müşteri ve {leadCount} potansiyel müşteri (lead) var. " +
                             $"Görev tablosunda henüz hiç başlanmamış (Bekliyor) {bekleyenGorevSayisi} görev, " +
                             $"ve şu an üzerinde çalışılan (İşlemde) {islemdekiGorevSayisi} görev bulunmaktadır.";

        // 3. Kullanıcı mesajı ve veritabanı bağlamını servise gönder
        var aiResponse = await _chatService.GetAiResponseAsync(request.Message, contextData);

        return Ok(new { response = aiResponse });
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}