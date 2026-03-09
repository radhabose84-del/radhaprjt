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
    }
}
