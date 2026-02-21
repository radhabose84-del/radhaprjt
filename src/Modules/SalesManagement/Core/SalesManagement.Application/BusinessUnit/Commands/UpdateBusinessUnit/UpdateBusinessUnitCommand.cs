#nullable disable

using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.BusinessUnit.Commands.UpdateBusinessUnit
{
    public class UpdateBusinessUnitCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string BusinessUnitName { get; set; }
        public string Description { get; set; }
        public int IsActive { get; set; }
    }
}
