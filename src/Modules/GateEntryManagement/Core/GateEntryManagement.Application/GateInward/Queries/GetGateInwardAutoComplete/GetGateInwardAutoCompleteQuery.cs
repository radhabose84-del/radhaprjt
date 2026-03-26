using GateEntryManagement.Application.GateInward.Dto;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Queries.GetGateInwardAutoComplete
{
    public sealed record GetGateInwardAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<GateInwardAutoCompleteDto>>;
}
