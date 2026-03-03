namespace SalesManagement.Application.Production.Dto
{
    public sealed class ProductionLookupDto
    {
        public int Id { get; set; }
        public string? PackNo { get; set; }
        public DateOnly PackDate { get; set; }
    }
}
