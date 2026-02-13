namespace Contracts.Dtos.Lookups.Users
{
    public class DivisionUnitLookupDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;

        public int DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;

        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }
}
