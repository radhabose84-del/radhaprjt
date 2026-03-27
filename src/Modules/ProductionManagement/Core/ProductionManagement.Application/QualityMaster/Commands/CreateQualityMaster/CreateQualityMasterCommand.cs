using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.QualityMaster.Commands.CreateQualityMaster
{
    public class CreateQualityMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? QualityName { get; set; }
        public string? Description { get; set; }
    }
}
