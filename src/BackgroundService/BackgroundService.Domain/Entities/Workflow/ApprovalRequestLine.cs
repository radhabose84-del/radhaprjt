using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.Domain.Entities.Workflow
{
    public class ApprovalRequestLine
    {
        public int Id { get; set; }
        public int ApprovalRequestId { get; set; }
        public int ModuleLineTransactionId { get; set; }
        public int StatusId { get; set; }
        public string Remark { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        public ApprovalRequest ApprovalRequest { get; set; }
        public MiscMaster Status { get; set; }
    }
}