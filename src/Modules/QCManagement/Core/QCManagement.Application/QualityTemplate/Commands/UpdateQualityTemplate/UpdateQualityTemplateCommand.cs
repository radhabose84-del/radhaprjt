using Contracts.Common;
using MediatR;

namespace QCManagement.Application.QualityTemplate.Commands.UpdateQualityTemplate
{
    public class UpdateQualityTemplateCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? TemplateName { get; set; }
        public string? Description { get; set; }
        public int IsActive { get; set; }
        public List<UpdateQualityTemplateParameterDto>? Parameters { get; set; }
    }

    public class UpdateQualityTemplateParameterDto
    {
        public int QualityParameterId { get; set; }
        public int SequenceNo { get; set; }
        public bool IsMandatory { get; set; } = true;
        public bool IsCritical { get; set; }
        public int? InspectionMethodId { get; set; }
        public int? SampleSize { get; set; }
        public int? SampleUomId { get; set; }
        public bool IsGradeApplicable { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
    }
}
