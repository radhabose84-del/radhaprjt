namespace Contracts.Dtos.Lookups.Inventory
{
    public class ItemLookupDto
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
    }
}
