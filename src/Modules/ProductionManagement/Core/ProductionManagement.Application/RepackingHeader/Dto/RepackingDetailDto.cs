namespace ProductionManagement.Application.RepackingHeader.Dto
{
    public class RepackingDetailDto
    {
        public int Id { get; set; }
        public int RepackHeaderId { get; set; }
        public int OldStartPackNo { get; set; }
        public int OldEndPackNo { get; set; }
        public decimal OldNetWeightPerPack { get; set; }
        public int OldTotalBags { get; set; }
        public decimal OldNetWeight { get; set; }
        public int OldWarehouseId { get; set; }
        public string? OldWarehouseName { get; set; }
        public int OldBinId { get; set; }
        public string? OldBinName { get; set; }
    }
}
