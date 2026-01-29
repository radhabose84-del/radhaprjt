using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Stock
{
    public class StockLedgerDto
    {
        public int UnitId { get; set; }
        public string DocType { get; set; } =string.Empty;
        public int DocNo { get; set; }
        public int DocSlNo { get; set; }
        public DateTime DocDate { get; set; }
        public int ItemId { get; set; }
        public int UomId { get; set; }
        public int WarehouseId { get; set; }
        public int StorageTypeId { get; set; }
        public int TargetId { get; set; }
        public decimal? ReceivedQty { get; set; }
        public decimal? ReceivedValue { get; set; }
        public decimal? IssueQty { get; set; }
        public decimal? IssueValue { get; set; }
    }
}