

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create
{
    public class CreatePOLocalReverseDto
    {
        public POLocalWorkFlowDto? Header { get; set; }
        public ICollection<POLocalWorkFlowDto>? Lines { get; set; }
    }
    
}