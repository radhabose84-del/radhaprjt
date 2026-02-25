namespace PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterAutoComplete
{
    public class TnCAutoCompleteDto
    {
         public int Id { get; set; }             // value
        public string TemplateName { get; set; } = ""; // text shown in UI
        public string? Code { get; set; }
    }
}