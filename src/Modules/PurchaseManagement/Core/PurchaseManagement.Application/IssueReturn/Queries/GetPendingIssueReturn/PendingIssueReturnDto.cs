using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturn
{
    public class PendingIssueReturnDto
    {
        public int Id { get; set; }
        public string? IssueReturnNo { get; set; }
        public DateTimeOffset IssueReturnDate { get; set; }
        public int RequestCategoryId { get; set; }
        public string? RequestCategoryName { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int? IssueHeaderId { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Remarks { get; set; }
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }

    }
}