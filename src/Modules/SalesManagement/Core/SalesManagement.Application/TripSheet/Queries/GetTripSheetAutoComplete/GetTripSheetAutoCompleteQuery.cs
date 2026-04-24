using MediatR;
using SalesManagement.Application.TripSheet.Dto;

namespace SalesManagement.Application.TripSheet.Queries.GetTripSheetAutoComplete
{
    public sealed record GetTripSheetAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<TripSheetLookupDto>>;
}
