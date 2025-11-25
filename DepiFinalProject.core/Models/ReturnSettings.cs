using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DepiFinalProject.core.Models
{
    public class ReturnSettings
    {
        public int ReturnWindowDays { get; set; } = 30;
        public List<string> AllowedReturnStatuses { get; set; } = new();
        public bool RestockOnApproval { get; set; } = true;
        public bool AutoRefundOnApproval { get; set; } = true;
        public List<string> AllowedReturnReasons { get; set; } = new();
        public string RefundProcessingNote { get; set; } = string.Empty;
    }
}
