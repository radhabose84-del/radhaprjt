using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    public class IndentDetail : BaseEntity
    {
        public int IndentHeaderId { get; set; }
        public int ItemId { get; set; }
        public int ItemCategoryId { get; set; }
        public int ItemUOMId { get; set; }
        public decimal? Rate { get; set; }
        public decimal QuantityRequired { get; set; }
        public DateOnly RequiredDate { get; set; }
        public decimal? TotalEstimatedCost { get; set; }
        public int PRConsumptionDays { get; set; }
        public string Remark { get; set; } = default!;
        // public decimal? ApprovedQuantity { get; set; }
        public bool IsRFQDone { get; set; }
        public int StatusId { get; set; }
        public decimal? POQty { get; set; }
        public IndentHeader IndentHeader { get; set; } = default!;
        public new MiscMaster Status { get; set; } = default!;
        
    }
}