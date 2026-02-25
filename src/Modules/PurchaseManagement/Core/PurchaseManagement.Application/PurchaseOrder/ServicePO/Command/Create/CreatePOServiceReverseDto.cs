namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create
{
    public class CreatePOServiceReverseDto
    {
        public POServiceWorkFlowDto Header { get; set; } = default!;
        public List<POServiceWorkFlowDto> Lines { get; set; } = new();
    }
}