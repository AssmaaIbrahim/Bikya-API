﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.DTOs.Orderdto
{
    public class UpdateOrderStatusDTO
    {
        public int OrderId { get; set; }
        public string NewStatus { get; set; } = string.Empty; // Paid / Completed / Cancelled
    }

}
