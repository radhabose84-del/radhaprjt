using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.CountGroup.Commands.CreateCountGroup
{
    public class CreateCountGroupCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? CountGroupCode { get; set; }
        public string? CountGroupName { get; set; }
        public string? Description { get; set; }
    }
}
