// PurchaseManagement.Application/Quotations/QuotationEntry/Queries/GetQuotationAutoComplete/GetQuotationAutoCompleteQuery.cs
using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using MediatR;

namespace PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetQuotationAutoComplete;

public class GetQuotationAutoCompleteQuery : IRequest<List<QuotationAutoCompleteDto>>
{
    public string? SearchPattern { get; set; }
}
