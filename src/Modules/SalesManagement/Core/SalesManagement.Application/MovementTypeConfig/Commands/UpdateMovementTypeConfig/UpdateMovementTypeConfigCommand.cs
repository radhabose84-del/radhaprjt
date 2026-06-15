using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.MovementTypeConfig.Commands.UpdateMovementTypeConfig
{
    public class UpdateMovementTypeConfigCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? MovementDescription { get; set; }
        public int MovementCategoryId { get; set; }
        public int FromStockTypeId { get; set; }
        public int ToStockTypeId { get; set; }
        public bool QuantityUpdateFlag { get; set; }
        public bool ValueUpdateFlag { get; set; }
        public string? AccountModifier { get; set; }
        public bool BatchRequiredFlag { get; set; }
        public bool NegativeStockAllowed { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
