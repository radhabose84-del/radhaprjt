using MediatR;
using PurchaseManagement.Application.PriceMaster.Dtos;
using Contracts.Common;

namespace PurchaseManagement.Application.PriceMaster.Commands.Update
{
    public sealed class UpdatePriceMasterCommand : IRequest<bool>, IRequirePermission
    {
        public PriceMasterUpdateDto Data { get; set; } = default!;
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
