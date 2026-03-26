using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.CountGroup.Commands.UpdateCountGroup
{
    public class UpdateCountGroupCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? CountGroupName { get; set; }
        public string? Description { get; set; }
        public int IsActive { get; set; }
    }
}
