namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create
{
    public class CreateImportPOReverseDto
    {
        public ImportPOWorkFlowDto? Header { get; set; }
        public ICollection<ImportPOWorkFlowDto>? Lines { get; set; }
    }
    
}   