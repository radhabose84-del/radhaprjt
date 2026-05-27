using Contracts.Common;
using MediatR;

namespace QCManagement.Application.QualityTemplate.Commands.CreateQualityTemplate
{
    public class CreateQualityTemplateCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? TemplateName { get; set; }
        public string? Description { get; set; }
        public List<CreateQualityTemplateParameterDto>? Parameters { get; set; }
    }

    public class CreateQualityTemplateParameterDto
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
    }
}
