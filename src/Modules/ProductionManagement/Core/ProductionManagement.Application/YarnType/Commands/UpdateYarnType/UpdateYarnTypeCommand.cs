using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.YarnType.Commands.UpdateYarnType
{
    public class UpdateYarnTypeCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? YarnTypeName { get; set; }
        public string? Description { get; set; }
        public int IsActive { get; set; }
    }
}
