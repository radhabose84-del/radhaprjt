using MediatR;
using PurchaseManagement.Application.OCREntry.Dto;

namespace PurchaseManagement.Application.OCREntry.Queries.GetOCREntryAutoComplete
{
    public sealed record GetOCREntryAutoCompleteQuery(string Term) : IRequest<IReadOnlyList<OCREntryLookupDto>>;
}
