using MediatR;
using PurchaseManagement.Application.PriceMaster.Dtos;
using Contracts.Common;

namespace PurchaseManagement.Application.PriceMaster.Commands.Create
{
    public sealed class CreatePriceMasterCommand : IRequest<int>, IRequirePermission
    {
        public PriceMasterCreateDto Data { get; init; } = default!;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
