namespace Contracts.Dtos.Lookups.Inventory
{
    public sealed class PriceGroupMasterLookupDto
    {
        public int Id { get; set; }
        public string PriceGroupCode { get; set; } = string.Empty;
        public string PriceGroupName { get; set; } = string.Empty;
    }
}
