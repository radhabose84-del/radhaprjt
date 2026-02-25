
using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit
{
    public class CreateBusinessUnitCommand : IRequest<ApiResponseDTO<int>>
    {
        public string BusinessUnitCode { get; set; } = null!;
        public string BusinessUnitName { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
