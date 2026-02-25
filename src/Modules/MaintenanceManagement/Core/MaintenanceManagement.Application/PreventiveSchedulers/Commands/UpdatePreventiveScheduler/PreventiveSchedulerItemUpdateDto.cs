namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.UpdatePreventiveScheduler
{
    public class PreventiveSchedulerItemUpdateDto
    {
        public int PreventiveSchedulerHeaderId { get; set; }
        public string? ItemId { get; set; }
        public int RequiredQty { get; set; }
        public string? OldCategoryDescription { get; set; }
        public string? OldGroupName { get; set; }
        public string? OldItemName { get; set; }
        
    }
}