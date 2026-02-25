namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServiceEntrySheetGetById
{
    public class ServiceEntryActivityDto
    {
        public int Id { get; set; }
        public int EntrySheetId { get; set; }
        public int? ActivityTypeId { get; set; }
        public string? Description { get; set; }
        public int PerformedById { get; set; }
        public string? PerformedByName { get; set; }
        public int? SESActivityStatusId { get; set; }
        public string? StatusRemarks { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
    }
}