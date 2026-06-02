namespace PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster
{
    public class TncApplicabilityDto
    {
        public int Id { get; set; }            // TnCTemplateApplicability.Id  (junction PK)
        public int TnCTemplateMasterId { get; set; } // FK to master (optional but handy)
        public int TransactionTypeId { get; set; }   // cross-module FK -> Finance.TransactionTypeMaster.Id
        public string? TypeName { get; set; }   // populated via ITransactionTypeLookup
        public string? ShortName { get; set; }  // populated via ITransactionTypeLookup
    }
}