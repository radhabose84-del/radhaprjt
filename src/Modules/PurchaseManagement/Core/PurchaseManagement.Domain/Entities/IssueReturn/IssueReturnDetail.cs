using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Domain.Entities.IssueReturn
{
    public class IssueReturnDetail
    {
        public int Id { get; set; }
        public int IssueReturnHeaderId { get; set; }
        public IssueReturnHeader IssueReturnHeaderDetails { get; set; } = null!;
        public int ItemId { get; set; }
        public int UomId { get; set; }
        public int WarehouseStockId { get; set; }
        public int? StorageTypeId { get; set; }
        public int? TargetId { get; set; }
        public decimal ReturnQuantity { get; set; }
        public decimal ReturnValue { get; set; }
        public int ReasonId { get; set; }
        public MiscMaster? IssueDetailReason { get; set; }
        public string? Remarks { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedDate { get; set; }
        public string? ApprovedByName { get; set; }
        public string? ApprovedIP { get; set; }
        public int StatusId { get; set; }
        public MiscMaster? StatusIssueDetailHeader { get; set; }
        public int? SubStoresDepartmentId { get; set; }


        
    }
}