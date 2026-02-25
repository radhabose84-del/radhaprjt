namespace Contracts.Dtos.Maintenance.Preventive
{
    public class WorkOrderItemSagaDto
    {
        public int WorkOrderId { get; set; }       
        public string OldItemCode { get; set; } = default!;
    }
}