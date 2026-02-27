using MediatR;

namespace SalesManagement.Application.OfficerAgent.Commands.DeleteOfficerAgent
{
    public sealed record DeleteOfficerAgentCommand(int Id) : IRequest<bool>;
}
