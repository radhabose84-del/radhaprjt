namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create
{
    public class POServiceWorkFlowDto
    {
        public int Id { get; set; }              // header / line id
        public string PONumber { get; set; } = default!;
        public int VendorId { get; set; }
        public int StatusId { get; set; }
        public int UnitId { get; set; }

       
    }
}