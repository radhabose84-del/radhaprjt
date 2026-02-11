namespace Contracts.Dtos.Lookups.Inventory
{
    public class MiscMasterLookupDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MiscTypeId { get; set; }
    }
}
