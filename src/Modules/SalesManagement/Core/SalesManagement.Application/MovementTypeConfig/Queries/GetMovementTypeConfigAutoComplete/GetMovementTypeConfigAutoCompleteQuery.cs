using MediatR;
using SalesManagement.Application.MovementTypeConfig.Dto;

namespace SalesManagement.Application.MovementTypeConfig.Queries.GetMovementTypeConfigAutoComplete
{
    public sealed record GetMovementTypeConfigAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<MovementTypeConfigLookupDto>>;
}
