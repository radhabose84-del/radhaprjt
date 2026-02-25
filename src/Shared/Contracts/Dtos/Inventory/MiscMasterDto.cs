namespace Contracts.Dtos.Inventory
{
    public class MiscMasterDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int MiscTypeId { get; set; } 
    }
}