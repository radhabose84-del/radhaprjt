namespace SalesManagement.Application.DispatchAdvice.Dto
{
    public class DispatchAdviceHeaderDto
    {
        public int Id { get; set; }
        public string? DispatchNo { get; set; }
        public DateOnly DispatchDate { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public int SalesOrderId { get; set; }
        public string? SalesOrderNo { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public decimal TotOrderQty { get; set; }
        public decimal TotDispatchedQty { get; set; }
        public decimal TotPendingQty { get; set; }
        public int? DispatchAddressId { get; set; }
        public string? DispatchAddressName { get; set; }
        public int DispatchTypeId { get; set; }
        public string? DispatchTypeName { get; set; }
        public int FreightId { get; set; }
        public string? FreightModeName { get; set; }
        public string? RateMethodName { get; set; }
        public decimal? FreightRate { get; set; }
        public int? TransporterId { get; set; }
        public string? TransporterName { get; set; }
        public string? VehicleNo { get; set; }
        public string? DriverName { get; set; }
        public string? LRNo { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public bool InvFlg { get; set; }
        public decimal Distance { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }

        // Nested details (for GetById)
        public List<DispatchAdviceDetailDto>? Details { get; set; }
    }
}
