using Contracts.Common;
using MediatR;
using QCManagement.Application.QualitySpecification.Dto;

namespace QCManagement.Application.QualitySpecification.Queries.GetAllQualitySpecification
{
    public class GetAllQualitySpecificationQuery : IRequest<ApiResponseDTO<List<QualitySpecificationListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? QualityTemplateId { get; set; }
        public int? ApplicableLevelId { get; set; }
        public int? QcTypeId { get; set; }
        public int? ItemCategoryId { get; set; }
        public int? ItemId { get; set; }
        public bool? IsActive { get; set; }
    }
}
