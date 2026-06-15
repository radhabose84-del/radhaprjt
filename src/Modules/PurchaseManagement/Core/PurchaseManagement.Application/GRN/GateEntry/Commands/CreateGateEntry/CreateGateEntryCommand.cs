using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.GRN.GateEntry.Commands.CreateGateEntry
{
    public class CreateGateEntryCommand : IRequest<int>, IRequirePermission
    {
        public CreateGateEntryDto GateEntryDetails { get; set; } = null!;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
