using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Domain.Entities.MRS
{
    public class MrsDetail
    {
        public int Id { get; set; }
        public int MrsHeaderId { get; set; }
        public MrsHeader MrsHeaderDetails { get; set; } = null!;
        public int ItemId { get; set; }
        public int UomId { get; set; }
        public decimal RequestQuantity { get; set; }
        public int? CostCenterId { get; set; }
        public int? FinanceCode { get; set; }
        public int WarehouseStockId { get; set; }

    }
}