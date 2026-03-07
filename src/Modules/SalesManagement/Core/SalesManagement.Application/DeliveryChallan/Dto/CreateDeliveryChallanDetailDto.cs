namespace SalesManagement.Application.DeliveryChallan.Dto
{
    public class CreateDeliveryChallanDetailDto
    {
        public int StoDetailId { get; set; }
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
    }
}
