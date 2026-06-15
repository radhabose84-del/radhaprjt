using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.CertificationMaster.Commands.DeleteCertificationMaster
{
    public sealed record DeleteCertificationMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
