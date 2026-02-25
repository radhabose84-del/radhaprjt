namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet
{
    public class ServicePOHeaderForSesDto
    {
        public int PurchaseOrderId { get; set; }
        public int ServicePoHeaderId { get; set; }
        public string PONumber { get; set; } = "";
        public DateTimeOffset PODate { get; set; }
        public int VendorId { get; set; }
        public int UnitId { get; set; }
        public int StatusId { get; set; }
    }
}