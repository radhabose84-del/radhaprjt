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
        public string? PartyAddress { get; set; }                // From SalesOrderHeader.PartyAddress

        public int? DispatchAddressId { get; set; }
        public string? DispatchAddressName { get; set; }
        public string? DispatchAddressLine1 { get; set; }
        public string? DispatchAddressLine2 { get; set; }
        public string? DispatchAddressPinCode { get; set; }

        public int? TransporterId { get; set; }
        public string? TransporterName { get; set; }

        public string? VehicleNo { get; set; }
        public string? DriverName { get; set; }
        public string? LRNo { get; set; }

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

        // Weight info (sourced from SalesOrderDetail + computed)
        public decimal BagWeight { get; set; }                   // From SalesOrderDetail.BagWeight
        public decimal TotalWeight { get; set; }                 // = TotalQty * BagWeight
        public int SaleUOMId { get; set; }
        public string? SaleUOMName { get; set; }
    }
}
