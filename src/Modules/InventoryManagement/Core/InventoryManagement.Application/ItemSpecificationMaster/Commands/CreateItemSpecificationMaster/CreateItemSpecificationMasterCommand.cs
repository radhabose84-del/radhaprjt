
using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationMaster.Commands.CreateItemSpecificationMaster
{
    public class CreateItemSpecificationMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? SpecificationCode { get; set; }
        public string? SpecificationName { get; set; }
        public int Order { get; set; }
    }
}
