using Contracts.Common;
using MediatR;
using SalesManagement.Application.ProductionPack.Dto;

namespace SalesManagement.Application.ProductionPack.Commands.CreateProduction
{
    public class CreateProductionCommand : IRequest<ApiResponseDTO<int>>
    {
        public CreateProductionDto? ProductionPackDetails { get; set; }
    }
}
