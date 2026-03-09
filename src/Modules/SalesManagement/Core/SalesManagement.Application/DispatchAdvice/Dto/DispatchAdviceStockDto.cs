namespace SalesManagement.Application.DispatchAdvice.Dto
{
    public class DispatchAdviceStockDto
    {
        public int Qty { get; set; }
        public decimal Value { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeCode { get; set; }
        public string? PackTypeName { get; set; }
        public decimal NetWeight { get; set; }
        public decimal TareWeight { get; set; }
        public decimal GrossWeight { get; set; }
        public int ConesPerBag { get; set; }
    }
}
