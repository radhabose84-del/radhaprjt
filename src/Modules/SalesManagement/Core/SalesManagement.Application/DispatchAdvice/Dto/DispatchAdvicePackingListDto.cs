namespace SalesManagement.Application.DispatchAdvice.Dto
{
    public sealed class DispatchAdvicePackingListDto
    {
        // Header — appears once
        public int DispatchAdviceId { get; set; }
        public string? DispatchNo { get; set; }
        public DateOnly DispatchDate { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }

        // Detail rows — one per pack
        public List<DispatchAdvicePackingListDetailDto> Details { get; set; } = [];
    }

    public sealed class DispatchAdvicePackingListDetailDto
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int LotId { get; set; }
        public string? LotCode { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public int PackNo { get; set; }
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public int? BinId { get; set; }
        public string? BinName { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalValue { get; set; }
    }
}
