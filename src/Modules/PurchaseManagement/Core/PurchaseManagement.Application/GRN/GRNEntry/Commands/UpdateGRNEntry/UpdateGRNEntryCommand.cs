using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry
{
    public class UpdateGRNEntryCommand : IRequest<bool>, IRequirePermission
    {
        public UpdateGRNEntryDto GrnEntryUpdate { get; set; } = null!;
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
