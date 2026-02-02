using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO
{
    public class IndentDetailsForPODto
    {
        public int Id { get; set; }
        public int IndentHeaderId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemCategoryId { get; set; }
        public string ItemCategoryName { get; set; }
        public int ItemUOMId { get; set; }
        public string UOMName { get; set; }
        public decimal QuantityRequired { get; set; }
        public DateOnly RequiredDate { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public int PRConsumptionDays { get; set; }
        public string Remark { get; set; }
        public decimal GSTPercentage { get; set; }
        public string HSNCode { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LastPOPrice { get; set; }
        public bool IsOnSpot { get; set; }
    }
}