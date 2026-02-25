namespace Contracts.Dtos.Maintenance.Preventive
{
    public class RollbackItemsDto
    {
        public int Id { get; set; }
        public int PreventiveSchedulerHeaderId { get; set; }
        public int ItemId { get; set; }
        public int RequiredQty { get; set; }
        public string OldItemId { get; set; } = default!;
        public string OldCategoryDescription { get; set; } = default!;
        public string OldGroupName { get; set; } = default!;
        public string? OldItemName { get; set; }
        
    }
}