namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create
{
    public class ImportPOWorkFlowDto
    {
        public int Id { get; set; }
        public string? PONumber { get; set; }
        public int VendorId { get; set; }
        public int StatusId { get; set; }
        public int UnitId { get; set; }
    }
}
