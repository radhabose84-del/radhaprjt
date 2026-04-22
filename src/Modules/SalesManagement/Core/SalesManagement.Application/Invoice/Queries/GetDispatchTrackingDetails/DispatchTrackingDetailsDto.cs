namespace SalesManagement.Application.Invoice.Queries.GetDispatchTrackingDetails
{
    public sealed class DispatchTrackingDetailsDto
    {
        public int SalesOrderId { get; set; }
        public string? SalesOrderNo { get; set; }

        public List<DispatchTrackingShipmentDto> Shipments { get; set; } = [];
    }

    public sealed class DispatchTrackingShipmentDto
    {
        // Dispatch Advice
        public int DispatchAdviceId { get; set; }
        public string? DispatchNo { get; set; }
        public DateOnly DispatchDate { get; set; }

        // Shipping Address (resolved from DispatchAddressMaster or PartyAddress Shipping type)
        public int? DispatchAddressId { get; set; }
        public string? DispatchAddressName { get; set; }
        public string? ShippingAddress { get; set; }

        // Party
        public int PartyId { get; set; }
        public string? PartyName { get; set; }

        // Invoice
        public int? InvoiceId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public string? VehicleNumber { get; set; }
        public string? TransporterName { get; set; }
        public string? LRNumber { get; set; }
        public DateOnly? LRDate { get; set; }

        // Item-wise details
        public List<DispatchTrackingItemDto> Items { get; set; } = [];
    }

    public sealed class DispatchTrackingItemDto
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public decimal NoOfBags { get; set; }
        public decimal BagWeight { get; set; }
        public decimal NetWeight { get; set; }
        public decimal RatePerKg { get; set; }
        public int? UOMId { get; set; }
        public string? UOMName { get; set; }
        public int? LotId { get; set; }
        public string? LotCode { get; set; }
        public int? PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
    }
}
