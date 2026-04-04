namespace SalesManagement.Application.SalesReturn.Dto
{
    public class CreateSalesReturnDetailDto
    {
        public int InvoiceHeaderId { get; set; }
        public int InvoiceDetailId { get; set; }
        public int ItemId { get; set; }
        public int? LotId { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public decimal ReturnQty { get; set; }
        public int? PackTypeId { get; set; }
        public int BagStatusId { get; set; }
    }
}
