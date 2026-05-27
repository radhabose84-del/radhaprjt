using Contracts.Common;
using MediatR;
using QCManagement.Application.QualityParameter.Dto;

namespace QCManagement.Application.QualityParameter.Queries.GetAllQualityParameter
{
    public class GetAllQualityParameterQuery : IRequest<ApiResponseDTO<List<QualityParameterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? ParameterGroupId { get; set; }
    }
}
