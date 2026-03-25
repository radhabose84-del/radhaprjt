using MediatR;

namespace GateEntryManagement.Application.GatePass.Commands.DeleteGatePass
{
    public sealed record DeleteGatePassCommand(int Id) : IRequest<bool>;
}
