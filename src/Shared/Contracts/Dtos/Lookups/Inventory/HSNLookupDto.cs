namespace Contracts.Dtos.Lookups.Inventory
{
    public class HSNLookupDto
    {
        public int Id { get; set; }
        public string HSNCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TypeId { get; set; }
        public string TypeCode { get; set; } = string.Empty;
        public decimal GSTPercentage { get; set; }
        public decimal CGSTPercentage { get; set; }
        public decimal SGSTPercentage { get; set; }
        public decimal IGSTPercentage { get; set; }
        public bool IsActive { get; set; }
    }
}
