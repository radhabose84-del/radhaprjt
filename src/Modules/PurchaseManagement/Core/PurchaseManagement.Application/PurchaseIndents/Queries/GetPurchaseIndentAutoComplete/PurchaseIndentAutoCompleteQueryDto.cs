namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentAutoComplete
{
    public class PurchaseIndentAutoCompleteQueryDto
    {
        public int Id { get; set; }
        public string IndentNumber { get; set; } = default!;
    }
}