using MediatR;

namespace GateEntryManagement.Application.GateInward.Commands.DeleteGateInward
{
    public sealed record DeleteGateInwardCommand(int Id) : IRequest<bool>;
}
