using MediatR;
using SalesManagement.Application.SalesOrganisation.Dto;

namespace SalesManagement.Application.SalesOrganisation.Queries.GetSalesOrganisationAutoComplete;

public sealed record GetSalesOrganisationAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<SalesOrganisationLookupDto>>;
