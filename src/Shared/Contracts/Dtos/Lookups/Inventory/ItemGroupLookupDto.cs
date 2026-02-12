namespace Contracts.Dtos.Lookups.Inventory
{
    public class ItemGroupLookupDto
    {
        public int Id { get; set; }
        public string ItemGroupCode { get; set; } = string.Empty;
        public string ItemGroupName { get; set; } = string.Empty;
    }
}
