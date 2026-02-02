namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create
{
    public class POLocalWorkFlowDto
    {
        public int Id { get; set; }
        public string? PONumber { get; set; }
        public int VendorId { get; set; }
        public int StatusId { get; set; }
        public int UnitId { get; set; }
    }
}
