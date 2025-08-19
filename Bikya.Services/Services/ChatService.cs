using Bikya.Data;
using Bikya.Data.Models;
using Bikya.Data.Response;
using Bikya.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bikya.Services.Services
{



    public class ChatService(BikyaContext db, ChatOrchestrator orch)
    {
        public async Task<ApiResponse<Guid>> CreateSessionAsync()
        {
            var s = new ChatSession();
            db.ChatSessions.Add(s);
            await db.SaveChangesAsync();
            return ApiResponse<Guid>.SuccessResponse(s.Id, "Session created");
        }

        public async Task<ApiResponse<string>> SendAsync(Guid sessionId, string text, CancellationToken ct = default)
        {
            var session = await db.ChatSessions.Include(x => x.Messages).FirstOrDefaultAsync(x => x.Id == sessionId, ct);
            if (session == null) return ApiResponse<string>.ErrorResponse("Session not found", 404);

            db.ChatMessages.Add(new ChatMessage { SessionId = sessionId, Role = "user", Text = text });
            var answer = await orch.HandleAsync(sessionId, text, ct);
            db.ChatMessages.Add(new ChatMessage { SessionId = sessionId, Role = "assistant", Text = answer });

            await db.SaveChangesAsync(ct);
            return ApiResponse<string>.SuccessResponse(answer);
        }
    }

}
