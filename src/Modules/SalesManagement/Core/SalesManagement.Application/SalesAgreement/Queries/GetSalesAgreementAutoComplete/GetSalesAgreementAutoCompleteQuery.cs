using MediatR;
using SalesManagement.Application.SalesAgreement.Dto;

namespace SalesManagement.Application.SalesAgreement.Queries.GetSalesAgreementAutoComplete
{
    public sealed record GetSalesAgreementAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<SalesAgreementLookupDto>>;
}
