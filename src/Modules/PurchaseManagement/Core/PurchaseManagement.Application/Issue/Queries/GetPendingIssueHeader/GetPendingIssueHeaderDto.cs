using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.Issue.Queries.GetPendingIssueHeader
{
    public class GetPendingIssueHeaderDto
    {
        public int MrsId { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int RequestCategoryId { get; set; }
        public string? RequestCategoryName { get; set; }
        public string? MrsNo { get; set; }
        public DateTimeOffset MrsDate { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int SubDepartmentId { get; set; }
        public string? SubDepartmentName { get; set; }
        public string? Remarks { get; set; }
        public int SubStoresWarehouseId { get; set; }
        public string? SubStoresWarehouseName { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal IssuedQuantity { get; set; }
        public decimal PendingQuantity { get; set; }
        public string? IssueStatus { get; set; }

    }
}