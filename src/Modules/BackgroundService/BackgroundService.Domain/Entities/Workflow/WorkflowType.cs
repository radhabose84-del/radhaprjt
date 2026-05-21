using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;

namespace BackgroundService.Domain.Entities.Workflow
{
    public class WorkflowType : BaseEntity
    {
        public int ModuleId { get; set; }
        public int MenuId { get; set; }
        public int? TransactionTypeId { get; set; }
        public byte HasLine { get; set; }
        public byte IsMultiselect  { get; set; }
        public ICollection<ApprovalStepDetail> ApprovalStepDetails { get; set; }
        public ICollection<ApprovalRule> ApprovalRules { get; set; }
        public ICollection<ApprovalRequest> ApprovalRequests { get; set; }
        
    }
}