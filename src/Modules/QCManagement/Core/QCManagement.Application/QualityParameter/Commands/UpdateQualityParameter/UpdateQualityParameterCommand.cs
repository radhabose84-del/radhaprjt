using Contracts.Common;
using MediatR;

namespace QCManagement.Application.QualityParameter.Commands.UpdateQualityParameter
{
    public class UpdateQualityParameterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? ParameterName { get; set; }
        public int ParameterGroupId { get; set; }
        public int? UnitId { get; set; }
        public string? Description { get; set; }
        public int IsActive { get; set; }
    }
}
