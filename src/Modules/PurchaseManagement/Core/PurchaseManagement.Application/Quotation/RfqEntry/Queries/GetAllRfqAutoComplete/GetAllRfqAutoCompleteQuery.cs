using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;
using MediatR;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqAutoComplete;

public class GetRfqAutoCompleteQuery : IRequest<List<RfqAutoCompleteDto>>
{
    public string? SearchPattern { get; set; }
    public DateOnly? LastSubmitDate { get; set; }
}