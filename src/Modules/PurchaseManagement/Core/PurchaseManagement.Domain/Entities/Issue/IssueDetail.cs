using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Domain.Entities.Issue
{
    public class IssueDetail
    {
        public int Id { get; set; }
        public int IssueHeaderId { get; set; }
        public IssueHeader MrsIssueDetails { get; set; } = null!;
        public int Sno { get; set; }
        public int ItemId { get; set; }
        public int UomId { get; set; }
        public decimal RequestQuantity { get; set; }
        public int WarehouseStockId { get; set; }
        public int StorageTypeId { get; set; }
        public int TargetId { get; set; }
        public decimal IssueQuantity { get; set; }
        public decimal IssueValue { get; set; }
        public int? CostCenterId { get; set; }
        public int? FinanceCode { get; set; }
      

    }
}