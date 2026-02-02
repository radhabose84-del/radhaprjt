namespace PurchaseManagement.Application.Quotation.RfqEntry.Dtos;

public class RfqAutoCompleteDto
{
    public int Id { get; set; }
    public string RfqCode { get; set; } = string.Empty;        
    public DateOnly? LastSubmitDate { get; set; }
}
