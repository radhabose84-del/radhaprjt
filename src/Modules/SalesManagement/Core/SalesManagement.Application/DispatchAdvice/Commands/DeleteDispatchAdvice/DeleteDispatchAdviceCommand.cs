using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.DispatchAdvice.Commands.DeleteDispatchAdvice
{
    public sealed record DeleteDispatchAdviceCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
