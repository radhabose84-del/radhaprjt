using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.MovementTypeConfig.Commands.CreateMovementTypeConfig
{
    public class CreateMovementTypeConfigCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? MovementCode { get; set; }
        public string? MovementDescription { get; set; }
        public int MovementCategoryId { get; set; }
        public int FromStockTypeId { get; set; }
        public int ToStockTypeId { get; set; }
        public bool QuantityUpdateFlag { get; set; } = true;
        public bool ValueUpdateFlag { get; set; }
        public string? AccountModifier { get; set; }
        public bool BatchRequiredFlag { get; set; }
        public bool NegativeStockAllowed { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
