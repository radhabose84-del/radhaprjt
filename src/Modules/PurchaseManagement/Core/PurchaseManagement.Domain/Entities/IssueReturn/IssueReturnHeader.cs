using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.Issue;

namespace PurchaseManagement.Domain.Entities.IssueReturn
{
    public class IssueReturnHeader
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? IssueReturnNo { get; set; }
        public DateTimeOffset IssueReturnDate { get; set; }
        public int? IssueHeaderId { get; set; }
        public IssueHeader IssueHeaderDetails { get; set; } = null!;
        public int DepartmentId { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedDate { get; set; }
        public string? ApprovedByName { get; set; }
        public string? ApprovedIP { get; set; }
        public string? Remarks { get; set; }
        public int StatusId { get; set; }
        public MiscMaster? StatusIssueHeader { get; set; }
        public ICollection<IssueReturnDetail>? IssueReturnDetailsHeaderName { get; set; }
        public int RequestCategoryId { get; set; }

    }
}