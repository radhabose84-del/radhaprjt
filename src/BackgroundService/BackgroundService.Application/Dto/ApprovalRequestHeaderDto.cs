using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Dto
{
    public class ApprovalRequestHeaderDto
    {
        public int ModuleTransactionId { get; set; }
        public string CurrentStatus { get; set; }
    }
}