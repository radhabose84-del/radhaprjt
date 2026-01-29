using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.Domain.Entities.Workflow
{
    public class ApprovalRequest
    {
        public int Id { get; set; }
        public string WorkflowType { get; set; }
        public int WorkflowTypeId { get; set; }
        public WorkflowType WType { get; set; }
        public int ModuleTransactionId { get; set; }
        public int ApprovalStepDetailId { get; set; }
        public int? ApprovalRuleId { get; set; }
        public int StatusId { get; set; }
        public DateTimeOffset RequestedDate { get; set; }
        public int UnitId { get; set; }
        public int? DepartmentId { get; set; }
        public string Remark { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        public string? Action { get; set; }
        public string ApproverBinding { get; set; }
        public string ApproverValue { get; set; }        
        public ApprovalStepDetail ApprovalStepDetail { get; set; }
        public ApprovalRule ApprovalRule { get; set; }
        public MiscMaster Status { get; set; }
        public ICollection<ApprovalDocument> ApprovalDocuments { get; set; }
        public ICollection<ApprovalRequestLine> ApprovalRequestLines { get; set; }
    }
}