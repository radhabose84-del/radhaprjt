using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndentById
{
    public class PendingIndentByIdDto
    {
        public int Id { get; set; }
        public string IndentNumber { get; set; }
        public DateOnly IndentDate { get; set; }
        public int IndentTypeId { get; set; }
        public string IndentTypeName { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public string Purpose { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int ApprovalRequestHeaderId { get; set; }
        public int ApproverId { get; set; }
        public string ApproverName { get; set; }
        // public string IsApprover { get; set; }
        public ICollection<PendingIndentDetailByIdDto> IndentDetails { get; set; }
        
    }
}