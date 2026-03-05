using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.StoTypeMaster.Commands.UpdateStoTypeMaster
{
    public class UpdateStoTypeMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? StoTypeName { get; set; }
        public string? Description { get; set; }
        public int PgiMovementTypeId { get; set; }
        public int GrMovementTypeId { get; set; }
        public int IsActive { get; set; }
    }
}
