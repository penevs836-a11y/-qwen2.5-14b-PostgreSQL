using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using AiShoppingAssistant;
using AiShoppingAssistant.Models;

Console.OutputEncoding = Encoding.UTF8;
using var db = new AppDbContext();
var aiService = new OllamaService();

while (true)
{
    Console.Clear();
    Console.WriteLine("==================================================");
    Console.WriteLine("=== ИИ-АНАЛИТИК ПО ПОКУПКАМ qwen2.5:14b + POSTGRES ===");
    Console.WriteLine("==================================================");
    Console.WriteLine("1. Задать глобальный вопрос ИИ (Анализ всей базы)");
    Console.WriteLine("2. Посмотреть историю советов из базы данных");
    Console.WriteLine("3. Выйти из приложения");
    Console.WriteLine("==================================================");
    Console.Write("Выбери действие (1-3): ");

    string? choice = Console.ReadLine();

    if (choice == "3")
    {
        Console.WriteLine("\nСпасибо за использование! До свидания.");
        break;
    }

    if (choice == "2")
    {
        Console.Clear();
        Console.WriteLine("=== ИСТОРИЯ СОХРАНЕННЫХ КОНСУЛЬТАЦИЙ ИИ ===");

        var history = await db.History.OrderByDescending(h => h.SavedAt).Take(5).ToListAsync();

        if (history.Count == 0)
        {
            Console.WriteLine("\nИстория пока пуста. Задайте ИИ первый вопрос.");
        }
        else
        {
            foreach (var item in history)
            {
                Console.WriteLine($"\n[Дата сохранения: {item.SavedAt}]");
                Console.WriteLine($"Запрос пользователя: \"{item.UserQuery}\"");
                Console.WriteLine($"Ответ qwen2.5:14b:\n{item.AiResponse}");
                Console.WriteLine(new string('-', 50));
            }
        }

        Console.WriteLine("\nНажми любую клавишу, чтобы вернуться в меню...");
        Console.ReadKey();
        continue;
    }

    if (choice == "1")
    {
        Console.Write("\nВведи твой вопрос (например: где лучше купить молоко?): ");
        string? userQuery = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userQuery))
        {
            Console.WriteLine("Запрос не может быть пустым. Нажми любую клавишу...");
            Console.ReadKey();
            continue;
        }

        Console.WriteLine("\n[1/3] Шаг C#: Извлекаю из PostgreSQL только нужные товары...");

       
        var storesMap = await db.Stores.ToDictionaryAsync(s => s.Id, s => s.StoreName);



       
        var words = userQuery.ToLower().Split(new[] { ' ', ',', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);

      
        var allProducts = await db.Products.ToListAsync();
        string searchTerm = "";

        foreach (var word in words)
        {
            if (word.Length >= 4 && allProducts.Any(p => p.ProductName.ToLower().Contains(word)))
            {
                searchTerm = word;
                break; 
            }
        }

        Console.WriteLine($"[ОТЛАДКА] Ищу в базе по ключевому слову: '{searchTerm}'");

        
        var foundProducts = await db.Products
            .Where(p => EF.Functions.ILike(p.ProductName, $"%{searchTerm}%"))
            .ToListAsync();






        string dataToSend = "";
        if (foundProducts.Count == 0 || userQuery.ToLower().Contains("закупк") || userQuery.ToLower().Contains("ужин"))
        {
            var storeTotals = allProducts
                .GroupBy(p => p.StoreId)
                .Select(g => new {
                    StoreName = storesMap.TryGetValue(g.Key, out var name) ? name : "Магазин",
                    TotalCost = g.Sum(p => p.Price)
                })
                .OrderBy(g => g.TotalCost);

            var sb = new StringBuilder();
            foreach (var store in storeTotals)
            {
                sb.AppendLine($"- Магазин: {store.StoreName}, Полная стоимость корзины из всех 42 товаров: {store.TotalCost} руб.");
            }
            dataToSend = sb.ToString();
            Console.WriteLine("Сгенерирован глобальный отчет по стоимости всей корзины.");
        }
        else
        {
           
            var sb = new StringBuilder();
            foreach (var p in foundProducts)
            {
                var storeName = storesMap.TryGetValue(p.StoreId, out var name) ? name : "Магазин";
                sb.AppendLine($"- Магазин: {storeName}, Товар: {p.ProductName}, Бренд: {p.Brand}, Цена: {p.Price} руб.");
            }
            dataToSend = sb.ToString();
            Console.WriteLine($"Найдено позиций в SQL под ваш запрос: {foundProducts.Count}");
        }

        Console.WriteLine("[2/3] Шаг ИИ: Передаю точечный контекст в Ollama (qwen2.5:14b)...");
        string aiResponse = await aiService.GetAiAdviceAsync(userQuery, dataToSend);

        Console.WriteLine("\n=== АНАЛИТИЧЕСКИЙ ОТЧЕТ ОТ ЛОКАЛЬНОЙ НЕЙРОСЕТИ ===");
        Console.WriteLine(aiResponse);
        Console.WriteLine("==================================================");

        Console.WriteLine("\n[3/3] Шаг Base: Фиксирую лог диалога в таблицу search_history...");

        var historyLog = new SearchHistory
        {
            UserQuery = userQuery,
            AiResponse = aiResponse,
            SavedAt = DateTime.UtcNow
        };

        db.History.Add(historyLog);
        await db.SaveChangesAsync();

        Console.WriteLine("Данные успешно сохранены в PostgreSQL! Нажми любую клавишу...");
        Console.ReadKey();
    }

}
