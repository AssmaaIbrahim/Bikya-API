using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.Data.Models
{
    public class FAQ
    {
        [Key] public int Id { get; set; }
        [MaxLength(300)] public string Question { get; set; } = "";
        public string Answer { get; set; } = "";
        public string? Tags { get; set; } // "returns,shipping"
    }
}
