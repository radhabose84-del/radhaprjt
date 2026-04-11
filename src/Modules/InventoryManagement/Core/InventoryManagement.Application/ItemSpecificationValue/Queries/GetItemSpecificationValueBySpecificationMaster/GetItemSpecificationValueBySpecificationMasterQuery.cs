using Contracts.Common;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueBySpecificationMaster
{
    public sealed record GetItemSpecificationValueBySpecificationMasterQuery(int SpecificationMasterId)
        : IRequest<ApiResponseDTO<List<ItemSpecificationValueDto>>>;
}
