namespace SalesManagement.Application.DispatchAdvice.Dto
{
    public class CreateDispatchAdviceDetailDto
    {
        public int SalesOrderDetailId { get; set; }
        public int ItemId { get; set; }
        public int LotId { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public decimal DispatchQty { get; set; }
        public int PackTypeId { get; set; }
    }
}
