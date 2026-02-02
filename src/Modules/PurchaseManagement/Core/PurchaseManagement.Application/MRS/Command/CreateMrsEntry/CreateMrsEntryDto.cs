using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.MRS.Command.CreateMrsEntry
{
    public class CreateMrsEntryDto
    {
        public int UnitId { get; set; }
        public int RequestCategoryId { get; set; }
        public int DepartmentId { get; set; }
        public int SubDepartmentId { get; set; }
        public int? SubStoresWarehouseId { get; set; }
        public string? Remarks { get; set; }
        public List<CreateMrsDetailDto> MrsDetails { get; set; } = new();
    }
    public class CreateMrsDetailDto
    {
        public int ItemId { get; set; }
        public int UomId { get; set; }
        public decimal RequestQuantity { get; set; }
        public int? CostCenterId { get; set; }
        public int? FinanceCode { get; set; }
        public int WarehouseStockId { get; set; }
    }
}
