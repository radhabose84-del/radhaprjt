namespace SalesManagement.Application.DeliveryChallan.Dto
{
    public class DeliveryChallanPrintDto
    {
        public DCPrintCompanyDto? Company { get; set; }
        public DCPrintRegisteredOfficeDto? RegisteredOffice { get; set; }
        public DCPrintHeaderDto? Header { get; set; }
        public DCPrintEWaybillDto? EWaybill { get; set; }
        public DCPrintPlantDto? From { get; set; }
        public DCPrintPlantDto? To { get; set; }
        public DCPrintTransportDto? Transport { get; set; }
        public List<DCPrintItemDto>? Items { get; set; }
        public DCPrintTotalsDto? Totals { get; set; }
    }

    public class DCPrintCompanyDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Gstin { get; set; }
        public string? Pan { get; set; }
        public string? Email { get; set; }
        public string? Web { get; set; }
        public string? Phone { get; set; }
    }

    public class DCPrintRegisteredOfficeDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
    }

    public class DCPrintHeaderDto
    {
        public string? DeliveryNumber { get; set; }
        public string? DeliveryDate { get; set; }
        public string? StoNumber { get; set; }
        public string? DcType { get; set; }
        public string? MovementCode { get; set; }
        public string? MovementDescription { get; set; }
        public string? Status { get; set; }
        public string? DateTimeOfSupply { get; set; }
    }

    public class DCPrintEWaybillDto
    {
        public string? EWBNumber { get; set; }
        public string? EwbStatus { get; set; }
        public string? GeneratedDate { get; set; }
    }

    public class DCPrintPlantDto
    {
        public string? UnitName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Phone { get; set; }
    }

    public class DCPrintTransportDto
    {
        public string? TransporterName { get; set; }
        public string? TransporterCode { get; set; }
        public string? TransporterGstin { get; set; }
        public string? VehicleNo { get; set; }
        public decimal? TransportDistance { get; set; }
    }

    public class DCPrintItemDto
    {
        public int SNo { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? LotNo { get; set; }
        public string? PackSerialNo { get; set; }
        public int? BagCount { get; set; }
        public int? BaleCount { get; set; }
        public decimal DispatchQuantity { get; set; }
        public string? UOMName { get; set; }
        public decimal NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal ExMillRate { get; set; }
        public decimal LineMovementValue { get; set; }
    }

    public class DCPrintTotalsDto
    {
        public int TotalBags { get; set; }
        public int TotalBales { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalNetWeight { get; set; }
        public decimal TotalGrossWeight { get; set; }
        public decimal DeliveryValue { get; set; }
        public decimal ConsignmentValue { get; set; }
        public string? DeliveryValueWords { get; set; }
        public string? Remarks { get; set; }
    }

    // --- Raw Dapper mapping DTOs (used by repository for SQL result mapping) ---

    public class DCPrintHeaderRawDto
    {
        public int Id { get; set; }
        public string? DeliveryNumber { get; set; }
        public DateOnly DeliveryDate { get; set; }
        public int StoHeaderId { get; set; }
        public string? StoNumber { get; set; }
        public int FromPlantId { get; set; }
        public int FromStorageLocationId { get; set; }
        public int ToPlantId { get; set; }
        public int ToStorageLocationId { get; set; }
        public int TransporterId { get; set; }
        public string? VehicleNumber { get; set; }
        public decimal? TransportDistance { get; set; }
        public decimal DeliveryValue { get; set; }
        public decimal ConsignmentValue { get; set; }
        public string? StatusName { get; set; }
        public string? DcTypeName { get; set; }
        public string? MovementCode { get; set; }
        public string? MovementDescription { get; set; }
        public string? Remarks { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
    }

    public class DCPrintDetailRawDto
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int LotId { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public decimal DispatchQuantity { get; set; }
        public int UOMId { get; set; }
        public int? BagCount { get; set; }
        public int? BaleCount { get; set; }
        public decimal NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal ExMillRate { get; set; }
        public decimal LineMovementValue { get; set; }
    }
}
