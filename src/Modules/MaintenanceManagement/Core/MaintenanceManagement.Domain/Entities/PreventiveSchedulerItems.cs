namespace MaintenanceManagement.Domain.Entities
{
    public class PreventiveSchedulerItems
    {
        public int Id { get; set; }
        public int PreventiveSchedulerHeaderId { get; set; }
        public required PreventiveSchedulerHeader PreventiveScheduler { get; set; }
        public int ItemId { get; set; }
        public int RequiredQty { get; set; }
        // public int? UnitId { get; set; }
        public string? OldItemId { get; set; }
        public string? OldCategoryDescription { get; set; }
        public string? OldGroupName { get; set; }
        public string? OldItemName{ get; set; }
    }
}