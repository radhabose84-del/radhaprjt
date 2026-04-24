namespace SalesManagement.Domain.Entities
{
    public class TripSheetDetail
    {
        public int Id { get; set; }
        public int TripSheetHeaderId { get; set; }          // FK → Sales.TripSheetHeader
        public int DispatchAdviceHeaderId { get; set; }     // FK → Sales.DispatchAdviceHeader
        public int SequenceNo { get; set; }

        // Navigation properties
        public TripSheetHeader? TripSheetHeader { get; set; }
        public DispatchAdviceHeader? DispatchAdviceHeader { get; set; }
    }
}
