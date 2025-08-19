using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Mscc.GenerativeAI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Bikya.Services.Providers
{


    public class OpenRouterProvider : IAIProvider
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _model;

        public OpenRouterProvider(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            //_apiKey = "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"
            //    ?? throw new Exception("OpenRouter API key missing");
            _apiKey = cfg["AI:OpenRouter:ApiKey"]
                      ?? throw new Exception("OpenRouter API key missing");

            _model = cfg["AI:OpenRouter:Model"] ?? "openai/gpt-3.5-turbo";
        }

        public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
        {
            try
            {
                using var req = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://openrouter.ai/api/v1/chat/completions"
                );

                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

                var payload = new
                {
                    model = _model,
                    messages = new[]
                    {
                    new { role = "user", content = prompt }
                },
                    max_tokens = 256
                };

                req.Content = JsonContent.Create(payload);

                using var res = await _http.SendAsync(req, ct);
                var json = await res.Content.ReadAsStringAsync(ct);

                if (!res.IsSuccessStatusCode)
                    return $"❌ OpenRouter HTTP error {res.StatusCode}: {json}";

                using var doc = JsonDocument.Parse(json);

                // OpenRouter بيرجع response في array داخل choices
                if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == JsonValueKind.Array &&
                    choices.GetArrayLength() > 0)
                {
                    var message = choices[0].GetProperty("message");
                    if (message.TryGetProperty("content", out var content))
                        return content.GetString() ?? "";
                }

                return $"⚠️ OpenRouter response unexpected format: {json}";
            }
            catch (HttpRequestException hre)
            {
                return $"❌ HTTP error from OpenRouter: {hre.Message}";
            }
            catch (Exception ex)
            {
                return $"❌ Unexpected error in OpenRouterProvider: {ex.Message}";
            }
        }

        public Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
        {
            // OpenRouter ممكن يدعم Embeddings بعدين، لو حابة نضيفها
            return Task.FromResult(Array.Empty<float>());
        }
    }
    //    public class HuggingFaceProvider : IAIProvider
    //    {
    //        private readonly HttpClient http;
    //        private readonly string _apiKey;
    //        private readonly string _model;

    //        public HuggingFaceProvider(HttpClient http, IConfiguration cfg)
    //        {
    //            this.http = http;

    //            _apiKey = cfg["AI:HuggingFace:ApiKey"] ?? "";
    //            //_model = cfg["AI:HuggingFace:Model"] ?? "google/flan-t5-small"; // نموذج مضمون متاح
    //            _model = "google/flan-t5-small";

    //        }

    //        public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    //        {
    //            try
    //            {
    //                using var req = new HttpRequestMessage(
    //    HttpMethod.Post,
    //    $"https://api-inference.huggingface.co/models/{_model}"
    //);
    //                req.Headers.Authorization = new("Bearer", _apiKey);


    //                var payload = new { inputs = prompt, parameters = new { max_new_tokens = 256 } };
    //                req.Content = JsonContent.Create(payload);

    //                var res = await http.SendAsync(req, ct);
    //                var json = await res.Content.ReadAsStringAsync(ct);

    //                if (!res.IsSuccessStatusCode)
    //                    return $"HuggingFace HTTP error {res.StatusCode}: {json}";

    //                using var doc = JsonDocument.Parse(json);

    //                if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
    //                {
    //                    var elem = doc.RootElement[0];
    //                    if (elem.TryGetProperty("generated_text", out var genText))
    //                        return genText.GetString() ?? "";
    //                }

    //                return $"HuggingFace response unexpected format: {json}";
    //            }
    //            catch (HttpRequestException hre)
    //            {
    //                return $"HTTP error from HuggingFace: {hre.Message}";
    //            }
    //            catch (Exception ex)
    //            {
    //                return $"Unexpected error in HuggingFaceProvider: {ex.Message}";
    //            }
    //        }

    //        public Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    //            => Task.FromResult(Array.Empty<float>()); // لو عايزة Embeddings استخدمي sentence-transformers endpoint
    //    }


    //public class HuggingFaceProvider(HttpClient http, IConfiguration cfg) : IAIProvider
    //{
    //    private readonly string _apiKey = cfg["AI:HuggingFace:ApiKey"] ?? "";
    //    private readonly string _model = cfg["AI:HuggingFace:Model"] ?? "mistralai/Mistral-7B-Instruct-v0.3";
    //    public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    //    {
    //        using var req = new HttpRequestMessage(HttpMethod.Post, $"https://api-inference.huggingface.co/models/{_model}");
    //        req.Headers.Authorization = new("Bearer", _apiKey);
    //        var payload = new { inputs = prompt, parameters = new { max_new_tokens = 256 } };
    //        req.Content = JsonContent.Create(payload);
    //        var res = await http.SendAsync(req, ct);
    //        res.EnsureSuccessStatusCode();
    //        var json = await res.Content.ReadAsStringAsync(ct);
    //        // HF غالبًا بيرجع مصفوفة كائنات فيها generated_text
    //        using var doc = JsonDocument.Parse(json);
    //        var generated = doc.RootElement[0].GetProperty("generated_text").GetString();
    //        return generated ?? "";
    //    }

    //    public Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    //        => Task.FromResult(Array.Empty<float>()); // لو عايزة Embeddings من HF استخدمي sentence-transformers endpoint
    //}
}
