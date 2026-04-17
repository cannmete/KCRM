using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class AiChatService
{
    private readonly HttpClient _httpClient;
    // OpenRouter üzerinden aldığın API Key buraya gelecek.
    private readonly string _apiKey = "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";

    public AiChatService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetAiResponseAsync(string userMessage, string contextData)
    {
        var url = "https://openrouter.ai/api/v1/chat/completions";

        var requestBody = new
        {
            model = "openai/gpt-oss-120b:free",
            messages = new[]
            {
            // Sistem mesajını burada zenginleştiriyoruz:
            new {
                role = "system",
                content = $"Sen bir CRM asistanısın. Türkçe konuş. {contextData} Bu bilgilere dayanarak kısa ve net cevaplar ver."
            },
            new { role = "user", content = userMessage }
        }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        // Yetkilendirme (Bearer Token)
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey.Trim());

        if (!_httpClient.DefaultRequestHeaders.Contains("HTTP-Referer"))
        {
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
            // Türkçe karakter (ı,ş,ğ vb.) KESİNLİKLE kullanmıyoruz
            _httpClient.DefaultRequestHeaders.Add("X-Title", "KCRM AI Chat");
        }

        var response = await _httpClient.PostAsync(url, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseString);

            // 1. Defansif Kontrol: Gelen JSON'da "choices" diye bir anahtar var mı?
            if (jsonDoc.RootElement.TryGetProperty("choices", out JsonElement choicesElement))
            {
                // Varsa, standart OpenAI/OpenRouter formatıdır, güvenle okuyabiliriz.
                var aiMessage = choicesElement[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return aiMessage ?? "Boş bir yanıt döndü.";
            }
            else if (jsonDoc.RootElement.TryGetProperty("error", out JsonElement errorElement))
            {
                // 2. Kontrol: OpenRouter "200 OK" dönüp içine gizlice hata mesajı koymuş olabilir
                return $"Model bir hata döndürdü: {errorElement.GetRawText()}";
            }
            else
            {
                // 3. Kontrol: Tamamen saçma ve beklemediğimiz bir format geldiyse
                // Ne geldiğini görelim ki sorunu çözelim!
                return $"API beklenmeyen bir JSON formatı gönderdi: {responseString}";
            }
        }

        // Eğer hata dönerse, konsola veya loglara detaylı hatayı yazdırmak test aşamasında hayat kurtarır
        var errorContent = await response.Content.ReadAsStringAsync();
        return $"Hata oluştu: {response.StatusCode} - {errorContent}";
    }
}