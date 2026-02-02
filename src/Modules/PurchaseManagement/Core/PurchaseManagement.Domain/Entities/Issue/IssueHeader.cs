using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.IssueReturn;
using PurchaseManagement.Domain.Entities.MRS;

namespace PurchaseManagement.Domain.Entities.Issue
{
    public class IssueHeader
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? IssueNo { get; set; }
        public DateTimeOffset IssueDate { get; set; }
        public int MrsHeaderId { get; set; }
        public MrsHeader MrsHeaderIssueDetails { get; set; } = null!;
        public int? SubStoresWarehouseId { get; set; }
        public int IssuedBy { get; set; }
        public DateTimeOffset? IssuedDate { get; set; }
        public string? IssuedByName { get; set; }
        public string? IssuedIp { get; set; }
        public string? Remarks { get; set; }
        public ICollection<IssueDetail>? IssueHeaderName { get; set; }
        public ICollection<IssueReturnHeader>? IssueReturnHeaderName { get; set; }
    }
}