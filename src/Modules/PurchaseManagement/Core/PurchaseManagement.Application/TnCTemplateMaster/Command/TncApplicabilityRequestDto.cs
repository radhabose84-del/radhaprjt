namespace PurchaseManagement.Application.TnCTemplateMaster.Command
{
    // Request-only payload for Create/Update applicabilities — exposes just the FK.
    // Display fields (Id, TnCTemplateMasterId, TypeName, ShortName) live on the response DTO.
    public class TncApplicabilityRequestDto
    {
        public int TransactionTypeId { get; set; }
    }
}
