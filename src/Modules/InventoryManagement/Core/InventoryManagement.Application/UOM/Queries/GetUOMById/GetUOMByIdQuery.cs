using Contracts.Common;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using MediatR;

namespace InventoryManagement.Application.UOM.Queries.GetUOMById
{
    public class GetUOMByIdQuery : IRequest<ApiResponseDTO<UOMDto>>
    {
        public int Id { get; set; }
   
        
    }
}