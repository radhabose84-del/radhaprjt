using Contracts.Common;
using MediatR;

namespace QCManagement.Application.QualityParameter.Commands.CreateQualityParameter
{
    public class CreateQualityParameterCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? ParameterName { get; set; }
        public int ParameterGroupId { get; set; }
        public int DataTypeId { get; set; }
        public int? UnitId { get; set; }
        public int ValidationTypeId { get; set; }
        public string? Description { get; set; }
    }
}
