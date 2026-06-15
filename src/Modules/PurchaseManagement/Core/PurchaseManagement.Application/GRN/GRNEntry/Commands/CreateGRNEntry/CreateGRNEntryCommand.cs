using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry;
using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands
{
    public class CreateGRNEntryCommand : IRequest<int>, IRequirePermission
    {
        public CreateGRNEntryDto GrnEntryCreate { get; set; } = null!;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
