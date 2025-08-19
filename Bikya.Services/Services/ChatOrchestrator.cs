using Bikya.Data;
using Bikya.Data.Enums;
using Bikya.Services.Providers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bikya.Services.Services
{

    public enum ChatIntent { OrderStatus, ProductSearch, FAQ, SmallTalk, CategorySearch }


    public class ChatOrchestrator
    {
        private readonly BikyaContext _db;
        private readonly IAIProvider _ai;

        public ChatOrchestrator(BikyaContext db, IAIProvider ai)
        {
            _db = db;
            _ai = ai;
        }

        public async Task<string> HandleAsync(Guid sessionId, string userText, CancellationToken ct = default)
        {
            bool isArabic = IsArabic(userText);

            // 1️⃣ AI-based Intent Classification
            var intentPrompt = $@"
You are a helpful assistant for a kids-products marketplace.
Classify the following user message into one of these intents:
- OrderStatus
- ProductSearch
- CategorySearch
- FAQ
- SmallTalk
Only return the intent name.

Message: ""{userText}""
";

            var intentResult = await _ai.GenerateAsync(intentPrompt, ct);
            var intent = intentResult.Trim() switch
            {
                "OrderStatus" => ChatIntent.OrderStatus,
                "ProductSearch" => ChatIntent.ProductSearch,
                "CategorySearch" => ChatIntent.CategorySearch,
                "FAQ" => ChatIntent.FAQ,
                _ => ChatIntent.SmallTalk
            };

            // 2️⃣ Handle each intent
            switch (intent)
            {
                case ChatIntent.OrderStatus:
                    var m = Regex.Match(userText, @"\b\d{4,}\b");
                    if (!m.Success)
                        return isArabic
                            ? "ممكن تديني رقم الطلب علشان أقدر أشيك على حالته؟"
                            : "Please provide your order number so I can check its status.";

                    var orderNo = int.Parse(m.Value);
                    var order = await _db.Orders
                        .AsNoTracking()
                        .FirstOrDefaultAsync(o => o.Id == orderNo, ct);

                    if (order == null)
                        return isArabic
                            ? $"معنديش طلب بالرقم {orderNo}، تأكد انه صحيح."
                            : $"I couldn't find an order with number {orderNo}, please check again.";

                    var orderPrompt = $@"
User asked about order #{orderNo}.
Order info: Status = {order.Status}, CreatedAt = {order.CreatedAt:d}
Write a concise reply in {(isArabic ? "Arabic" : "English")}.
";
                    return await _ai.GenerateAsync(orderPrompt, ct);

                case ChatIntent.ProductSearch:
                    var products = await _db.Products
                        .AsNoTracking()
                        .Where(p => p.Title.Contains(userText) || p.Description.Contains(userText))
                        .Take(5)
                        .Select(p => new { p.Title, p.Price })
                        .ToListAsync(ct);

                    var productPrompt = products.Any()
                        ? $@"
User asked about products: ""{userText}""
Matching products:
{string.Join("\n- ", products.Select(p => $"{p.Title} - {p.Price} EGP"))}
Write a friendly reply in {(isArabic ? "Arabic" : "English")}.
"
                        : $@"
No matching products found for: ""{userText}""
Write a friendly suggestion reply in {(isArabic ? "Arabic" : "English")} to help the user.
";

                    return await _ai.GenerateAsync(productPrompt, ct);

                case ChatIntent.CategorySearch:
                    var cats = await _db.Categories
                        .AsNoTracking()
                        .Where(c => c.Name.Contains(userText))
                        .Select(c => c.Name)
                        .ToListAsync(ct);

                    var catPrompt = cats.Any()
                        ? $@"
User asked about categories: ""{userText}""
Matching categories:
{string.Join("\n- ", cats)}
Write a concise reply in {(isArabic ? "Arabic" : "English")}.
"
                        : $@"
No matching categories found for: ""{userText}""
Write a friendly suggestion reply in {(isArabic ? "Arabic" : "English")}.
";

                    return await _ai.GenerateAsync(catPrompt, ct);

                case ChatIntent.FAQ:
                    var faqs = await _db.FAQs.AsNoTracking().ToListAsync(ct);
                    var best = faqs
                        .Select(f => new { f, score = SimpleSimilarity(userText, f.Question) })
                        .OrderByDescending(x => x.score)
                        .FirstOrDefault();

                    if (best is { score: > 0.2 })
                        return isArabic ? best.f.Answer : await _ai.GenerateAsync($"Translate this to English: {best.f.Answer}", ct);

                    var faqPrompt = $@"
User asked: ""{userText}""
Answer concisely based on kids-products marketplace information in {(isArabic ? "Arabic" : "English")}.
";
                    return await _ai.GenerateAsync(faqPrompt, ct);

                default: // SmallTalk
                    var smallTalkPrompt = $@"
You are a helpful assistant for a kids-products marketplace.
Reply naturally in {(isArabic ? "Arabic" : "English")} to the user message:
""{userText}""
";
                    return await _ai.GenerateAsync(smallTalkPrompt, ct);
            }
        }

        private static double SimpleSimilarity(string a, string b)
        {
            var A = Tokenize(a); var B = Tokenize(b);
            if (A.Count == 0 || B.Count == 0) return 0;
            var inter = A.Intersect(B).Count();
            return (double)inter / Math.Sqrt(A.Count * B.Count);
        }

        private static HashSet<string> Tokenize(string s)
            => new(s.ToLower().Split(' ', '-', '.', '،', ',', '!', '?').Where(x => x.Length > 1));

        private static bool IsArabic(string text)
            => text.Any(c => c >= 0x0600 && c <= 0x06FF);
    }
   
}
