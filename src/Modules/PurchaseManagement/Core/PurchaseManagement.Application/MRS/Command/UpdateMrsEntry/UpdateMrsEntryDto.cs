namespace PurchaseManagement.Application.MRS.Command.UpdateMrsEntry
{
    public class UpdateMrsEntryDto
    {
        public int Id { get; set; } // MRS Header Id
        public int RequestCategoryId { get; set; }
        public int DepartmentId { get; set; }
        public int SubDepartmentId { get; set; }
        public int? SubStoresWarehouseId { get; set; }
        public string? Remarks { get; set; }
        public List<UpdateMrsDetailDto> UpdateMrsDetails { get; set; } = new();


        public class UpdateMrsDetailDto
        {
            public int MrsHeaderId { get; set; }
            public int ItemId { get; set; }
            public int UomId { get; set; }
            public decimal RequestQuantity { get; set; }
            public int? CostCenterId { get; set; }
            public int? FinanceCode { get; set; }
            public int WarehouseStockId { get; set; }
    }
}
}