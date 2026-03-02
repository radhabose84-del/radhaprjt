namespace SalesManagement.Application.LotMaster.Dto
{
    public sealed class LotMasterLookupDto
    {
        public int Id { get; set; }
        public string? LotCode { get; set; }
        public string? BatchNumber { get; set; }
    }
}
