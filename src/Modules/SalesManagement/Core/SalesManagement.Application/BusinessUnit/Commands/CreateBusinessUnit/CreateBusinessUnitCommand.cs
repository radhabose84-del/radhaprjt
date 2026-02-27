
using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit
{
    public class CreateBusinessUnitCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? BusinessUnitCode { get; set; }
        public string? BusinessUnitName { get; set; }
        public string? Description { get; set; }
    }
}
