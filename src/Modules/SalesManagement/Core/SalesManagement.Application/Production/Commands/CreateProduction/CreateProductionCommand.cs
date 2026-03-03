using Contracts.Common;
using MediatR;
using SalesManagement.Application.Production.Dto;

namespace SalesManagement.Application.Production.Commands.CreateProduction
{
    public class CreateProductionCommand : IRequest<ApiResponseDTO<int>>
    {
        public CreateProductionDto? ProductionPackDetails { get; set; }
    }
}
