using Contracts.Common;
using MediatR;

namespace QCManagement.Application.QualitySpecification.Commands.CreateQualitySpecification
{
    public class CreateQualitySpecificationCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? SpecificationName { get; set; }
        public int QualityTemplateId { get; set; }
        public int ApplicableLevelId { get; set; }
        public int QcTypeId { get; set; }
        public int? ItemCategoryId { get; set; }
        public int? ItemId { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
        public List<CreateQualitySpecificationParameterDto>? Parameters { get; set; }
    }

    public class CreateQualitySpecificationParameterDto
    {
        public int QualityParameterId { get; set; }
        public int ValidationTypeId { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? ExpectedValue { get; set; }
        public List<string>? AllowedValues { get; set; }
        public int? SeverityId { get; set; }
        public int? FailureActionId { get; set; }
        public bool IsSamplingRequired { get; set; }
        public string? Remarks { get; set; }
    }
}
