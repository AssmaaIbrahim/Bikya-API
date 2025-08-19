using Bikya.Data;
using Bikya.Data.Models;
using Bikya.Data.Repositories.Interfaces;
using Bikya.Data.Response;
using Bikya.DTOs.CategoryDTOs;
using Bikya.Services.Interfaces;
using Bikya.Services.Services;
using Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bikya.API.Areas.Chatbot
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("ChatMessage")]

    public class ChatController(ChatService svc) : ControllerBase
    {
        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession()
        {
            var res = await svc.CreateSessionAsync();
            return StatusCode(res.StatusCode, res);
        }

        public record SendDto(Guid SessionId, string Text);

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendDto dto, CancellationToken ct)
        {
            var res = await svc.SendAsync(dto.SessionId, dto.Text, ct);
            return StatusCode(res.StatusCode, res);
        }
    }
}
//    public class ChatController : ControllerBase
//    {
//        private readonly IChatSessionService _chatSessionService;
//        private readonly IChatMessageService _chatMessageService;
//        private readonly IHuggingFaceService _huggingFaceService;
//        private readonly BikyaContext _db;

//        public ChatController(
//            IChatSessionService chatSessionService,
//            IChatMessageService chatMessageService,
//            IHuggingFaceService huggingFaceService,
//            BikyaContext db)
//        {
//            _chatSessionService = chatSessionService;
//            _chatMessageService = chatMessageService;
//            _huggingFaceService = huggingFaceService;
//            _db = db;
//        }

//        // Start a chat session
//        [HttpPost("start")]
//        public async Task<IActionResult> StartSession([FromQuery] int? userId)
//        {
//            var session = await _chatSessionService.CreateSessionAsync(userId);

//            // If user not logged in
//            if (userId == null)
//            {
//                var categories = await _db.Categories
//                    .Select(c => c.Name)
//                    .ToListAsync();

//                var introMessage = "Hello! You are chatting as a guest. You can browse the site categories below, and sign up for more features:\n" +
//                                   $"- Categories: {string.Join(", ", categories)}\n" +
//                                   "To register, click the Sign Up button at the top of the page.";

//                return Ok(new
//                {
//                    success = true,
//                    message = introMessage,
//                    sessionId = session.Id
//                });
//            }

//            return Ok(new
//            {
//                success = true,
//                message = "Chat session started.",
//                sessionId = session.Id
//            });
//        }

//        // Send a message in the session
//        [HttpPost("message")]
//        public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequestDto request)
//        {
//            var session = await _chatSessionService.GetSessionByIdAsync(request.SessionId);
//            if (session == null)
//            {
//                return NotFound(new { success = false, message = "Session not found." });
//            }

//            // Save user message
//            await _chatMessageService.AddMessageAsync(session.Id, request.Message, "user");

//            // Get AI response
//            var aiReply = await _huggingFaceService.GetAIResponseAsync(request.Message);

//            // Save AI reply
//            await _chatMessageService.AddMessageAsync(session.Id, aiReply, "ai");

//            return Ok(new
//            {
//                success = true,
//                userMessage = request.Message,
//                aiMessage = aiReply
//            });
//        }
//    }
//}
//public class MessageRequest
//    {
//        public string Sender { get; set; } = "User";
//        public string Content { get; set; }
//    }
//public class ChatMessageController : ControllerBase
//{
//    private readonly IChatMessageService _messageService;

//    public ChatMessageController(IChatMessageService messageService)
//    {
//        _messageService = messageService;
//    }

//    [HttpGet("{sessionId}")]
//    public async Task<IActionResult> GetMessages(Guid sessionId)
//    {
//        var response = await _messageService.GetMessagesBySessionAsync(sessionId);
//        return StatusCode(response.StatusCode, response);
//    }

//    [HttpPost]
//    public async Task<IActionResult> AddMessage(Guid sessionId, [FromBody] string message)
//    {
//        var response = await _messageService.AddMessageAsync(sessionId, "User", message);
//        return StatusCode(response.StatusCode, response);
//    }
//}

