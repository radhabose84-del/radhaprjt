namespace SalesManagement.Application.TripSheet.Dto
{
    public class TripSheetDetailDto
    {
        public int Id { get; set; }
        public int TripSheetHeaderId { get; set; }
        public int SequenceNo { get; set; }
        public int DispatchAdviceHeaderId { get; set; }
        public string? DispatchNo { get; set; }            // JOIN DispatchAdviceHeader
        public DateOnly DispatchDate { get; set; }         // JOIN DispatchAdviceHeader
        public string? VehicleNo { get; set; }             // JOIN DispatchAdviceHeader
        public decimal TotDispatchedQty { get; set; }      // JOIN DispatchAdviceHeader
        public int PartyId { get; set; }                   // JOIN DispatchAdviceHeader
        public string? CustomerName { get; set; }          // Cross-module (IPartyLookup)
        public string? City { get; set; }                  // Cross-module (IPartyLookup)
        public string? OrderNo { get; set; }               // JOIN SalesOrderHeader
        public bool InvFlg { get; set; }                   // JOIN DispatchAdviceHeader
        public int? InvoiceHeaderId { get; set; }          // LEFT JOIN InvoiceHeader
        public string? InvoiceNo { get; set; }             // LEFT JOIN InvoiceHeader
    }
}
