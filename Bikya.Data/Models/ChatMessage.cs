using Mscc.GenerativeAI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.Data.Models
{
    public class ChatMessage
    {
        [Key] public int Id { get; set; }
        public Guid SessionId { get; set; }
        public ChatSession? Session { get; set; }
        public string Role { get; set; } = "user"; // user | assistant | system
        public string Text { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
