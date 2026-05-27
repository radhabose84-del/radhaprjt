using Contracts.Common;
using MediatR;
using QCManagement.Application.QualityTemplate.Dto;

namespace QCManagement.Application.QualityTemplate.Queries.GetAllQualityTemplate
{
    public class GetAllQualityTemplateQuery : IRequest<ApiResponseDTO<List<QualityTemplateListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
    }
}
