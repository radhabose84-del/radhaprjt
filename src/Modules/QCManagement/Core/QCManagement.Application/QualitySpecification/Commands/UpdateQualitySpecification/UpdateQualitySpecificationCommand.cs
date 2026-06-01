using Contracts.Common;
using MediatR;

namespace QCManagement.Application.QualitySpecification.Commands.UpdateQualitySpecification
{
    public class UpdateQualitySpecificationCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? SpecificationName { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
        public int QcTypeId { get; set; }
        public int IsActive { get; set; }
        public List<UpdateQualitySpecificationParameterDto>? Parameters { get; set; }
    }

    public class UpdateQualitySpecificationParameterDto
    {
        public int Id { get; set; }
        public int ValidationTypeId { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? ExpectedValue { get; set; }
        public List<string>? AllowedValues { get; set; }
        public int? SeverityId { get; set; }
        public int? FailureActionId { get; set; }
        public bool IsSamplingRequired { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
    }
}
