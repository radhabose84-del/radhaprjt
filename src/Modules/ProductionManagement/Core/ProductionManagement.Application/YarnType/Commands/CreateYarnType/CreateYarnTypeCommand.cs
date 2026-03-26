using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.YarnType.Commands.CreateYarnType
{
    public class CreateYarnTypeCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? YarnTypeCode { get; set; }
        public string? YarnTypeName { get; set; }
        public string? Description { get; set; }
    }
}
