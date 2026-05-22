using Contracts.Common;
using InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion;
using MediatR;

namespace InventoryManagement.Application.UOMConversion.Command.UpdateUOMConversion
{
    public class UpdateUOMConversionCommand   : IRequest<ApiResponseDTO<UOMConversionDto>>, IRequirePermission
    {
         public int Id { get; set; }                
        public int FromUOMId { get; set; }         
        public int ToUOMId { get; set; }           
        public decimal ConversionValue { get; set; } 
        public byte IsActive { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
