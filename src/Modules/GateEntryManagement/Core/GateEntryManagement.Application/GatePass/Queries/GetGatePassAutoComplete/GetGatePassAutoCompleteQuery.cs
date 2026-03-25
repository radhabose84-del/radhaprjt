using GateEntryManagement.Application.GatePass.Dto;
using MediatR;

namespace GateEntryManagement.Application.GatePass.Queries.GetGatePassAutoComplete
{
    public sealed record GetGatePassAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<GatePassAutoCompleteDto>>;
}
