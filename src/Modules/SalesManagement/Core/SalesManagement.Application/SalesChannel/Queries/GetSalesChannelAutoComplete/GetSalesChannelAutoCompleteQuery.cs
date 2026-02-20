#nullable disable
using MediatR;
using SalesManagement.Application.SalesChannel.Dto;

namespace SalesManagement.Application.SalesChannel.Queries.GetSalesChannelAutoComplete;

public sealed record GetSalesChannelAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<SalesChannelLookupDto>>;
