namespace Contracts.Dtos.Inventory
{
    public class ItemGroupDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string ItemGroupCode { get; set; } = default!;
        public string ItemGroupName { get; set; } = default!;
        public bool IsActive { get; set; }       
       
    }
}