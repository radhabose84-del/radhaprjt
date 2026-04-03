namespace ProductionManagement.Application.RepackingHeader.Dto
{
    public sealed class RepackingHeaderLookupDto
    {
        public int Id { get; set; }
        public string? RepackDocNo { get; set; }
        public DateOnly RepackDate { get; set; }
    }
}
