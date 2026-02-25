using Contracts.Common;
using InventoryManagement.Application.HSNMaster.Queries.GetAllHSNMaster;
using MediatR;

namespace InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterById
{
    public class GetHSNMasterByIdQuery : IRequest<ApiResponseDTO<HSNMasterDto>>
    {
        public int Id { get; set; }
    }
}