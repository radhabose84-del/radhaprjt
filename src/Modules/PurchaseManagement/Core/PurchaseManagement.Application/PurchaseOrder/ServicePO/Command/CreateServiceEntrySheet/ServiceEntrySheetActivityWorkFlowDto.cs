namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet
{
    public class ServiceEntrySheetActivityWorkFlowDto
    {
         public int Id { get; set; }                  // Activity Id
        public int? ActivityTypeId { get; set; }
        public int PerformedById { get; set; }
        public int? SESActivityStatusId { get; set; }
    }
}