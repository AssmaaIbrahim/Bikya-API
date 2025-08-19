using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.Services.Providers
{
    public interface IAIProvider
    {
        Task<string> GenerateAsync(string prompt, CancellationToken ct = default);
        Task<float[]> EmbedAsync(string text, CancellationToken ct = default); // لو الموديل بيدعم Embeddings
    }
}
