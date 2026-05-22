using Contracts.Common;
using InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion;
using MediatR;

namespace InventoryManagement.Application.UOMConversion.Command.CreateUOMConversion
{
    public class CreateUOMConversionCommand : IRequest<ApiResponseDTO<UOMConversionDto>>, IRequirePermission
    {
        public int FromUOMId { get; set; }
        public int ToUOMId { get; set; }
        public decimal ConversionValue { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
