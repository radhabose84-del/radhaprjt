namespace SalesManagement.Application.DispatchAdvice.Dto
{
    public class DispatchAdviceDetailDto
    {
        public int Id { get; set; }
        public int DispatchAdviceHeaderId { get; set; }
        public int SalesOrderDetailId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int LotId { get; set; }
        public string? LotCode { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public decimal DispatchQty { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }

        // From SalesOrderDetail
        public int? HSNId { get; set; }
        public string? HSNCode { get; set; }
        public decimal ExMillRate { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TCSPercentage { get; set; }
        public decimal TCSAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal BagWeight { get; set; }
    }
}
