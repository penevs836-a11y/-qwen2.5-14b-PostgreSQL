using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AiShoppingAssistant;

public class OllamaService
{
    private readonly HttpClient _httpClient = new();

    public async Task<string> GetAiAdviceAsync(string userQuery, string pricesData)
    {
        var url = "http://localhost:11434/api/generate";

        var prompt = $"Ты — финансовый ассистент по закупкам. Пользователь задал конкретный вопрос: \"{userQuery}\".\n" +
             $"Вот данные, которые C# вытащил из базы данных PostgreSQL специально под этот запрос:\n{pricesData}\n" +
             "ТВОЯ ЗАДАЧА:\n" +
             "1. Ответь СТРОГО на вопрос пользователя, используя ТОЛЬКО предоставленные данные выше.\n" +
             "2. Напиши, в каком магазине этот товар самый дешевый, а в каком самый дорогой.\n" +
             "3. Посчитай точную разницу в рублях между самым дешевым и дорогим вариантом.\n" +
             "4. Если пользователь спросил про конкретный товар (например, молоко), пиши ТОЛЬКО про этот товар! Никакого кофе!\n" +
             "ОБЯЗАТЕЛЬНОЕ ТРЕБОВАНИЕ: Напиши весь ответ строго на РУССКОМ языке, кратко (до 4 строк), без лишней воды.";



        var requestBody = new
        {
            model = "qwen2.5:14b",



            prompt = prompt,
            stream = false
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseText = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseText);
            return doc.RootElement.GetProperty("response").GetString() ?? "Не удалось получить ответ от ИИ.";
        }
        catch (Exception ex)
        {
            return $"Ошибка при обращении к Ollama: {ex.Message}. Убедись, что сервер Ollama запущен в Windows.";
        }
    }
}
