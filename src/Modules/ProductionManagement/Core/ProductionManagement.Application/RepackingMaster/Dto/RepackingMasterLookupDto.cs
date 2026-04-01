namespace ProductionManagement.Application.RepackingMaster.Dto
{
    public sealed class RepackingMasterLookupDto
    {
        public int Id { get; set; }
        public string? RepackDocNo { get; set; }
        public DateOnly RepackDate { get; set; }
    }
}
