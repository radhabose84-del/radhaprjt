namespace SalesManagement.Application.FreightMaster.Dto
{
    public sealed class FreightMasterLookupDto
    {
        public int Id { get; set; }
        public string? FreightModeName { get; set; }
        public string? RateMethodName { get; set; }
        public decimal Rate { get; set; }
    }
}
