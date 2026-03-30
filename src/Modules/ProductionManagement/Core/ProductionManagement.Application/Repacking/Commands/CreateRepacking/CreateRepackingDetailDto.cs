namespace ProductionManagement.Application.Repacking.Commands.CreateRepacking
{
    public class CreateRepackingDetailDto
    {
        public int ItemId { get; set; }
        public int LotId { get; set; }
        public int BinId { get; set; }
        public int WarehouseId { get; set; }
        public int PackTypeId { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public int OldPackDetailId { get; set; }
    }
}
