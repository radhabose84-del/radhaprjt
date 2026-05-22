using MediatR;

namespace UserManagement.Application.AccessPolicy.Commands.RemoveRoleAccessPolicy
{
    public sealed record RemoveRoleAccessPolicyCommand(int Id) : IRequest<bool>;
}
