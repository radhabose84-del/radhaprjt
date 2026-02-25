using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.HSNMaster.Command.UpdateHSNMaster
{
    public class UpdateHSNMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string HSNCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int GSTCategoryId { get; set; }
        public decimal GSTPercentage { get; set; }
        public decimal IGSTPercentage { get; set; }
        public DateTimeOffset ValidFrom { get; set; }        
        public byte IsActive { get; set; }
        
    }
}