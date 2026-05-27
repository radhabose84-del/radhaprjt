using QCManagement.Application.QualityTemplate.Commands.CreateQualityTemplate;
using QCManagement.Application.QualityTemplate.Commands.UpdateQualityTemplate;
using QCManagement.Application.QualityTemplate.Dto;

namespace QCManagement.UnitTests.TestData
{
    public static class QualityTemplateBuilders
    {
        public static CreateQualityTemplateCommand ValidCreateCommand(
            string? name = "Yarn QC - Standard Cotton",
            string? description = "Standard QC checks for cotton yarn",
            List<CreateQualityTemplateParameterDto>? parameters = null) =>
            new CreateQualityTemplateCommand
            {
                TemplateName = name,
                Description = description,
                Parameters = parameters ?? ValidCreateParameterList()
            };

        public static List<CreateQualityTemplateParameterDto> ValidCreateParameterList() =>
            new List<CreateQualityTemplateParameterDto>
            {
                new CreateQualityTemplateParameterDto
                {
                    QualityParameterId = 1,
                    SequenceNo = 1,
                    IsMandatory = true,
                    IsCritical = true,
                    InspectionMethodId = 18,
                    SampleSize = 5,
                    SampleUomId = 1,
                    IsGradeApplicable = true,
                    Remarks = "Lab tensile test"
                },
                new CreateQualityTemplateParameterDto
                {
                    QualityParameterId = 2,
                    SequenceNo = 2,
                    IsMandatory = true,
                    IsCritical = false,
                    InspectionMethodId = 17,
                    IsGradeApplicable = false
                }
            };

        public static UpdateQualityTemplateCommand ValidUpdateCommand(
            int id = 1,
            string? name = "Updated Template Name",
            string? description = "Updated description",
            int isActive = 1,
            List<UpdateQualityTemplateParameterDto>? parameters = null) =>
            new UpdateQualityTemplateCommand
            {
                Id = id,
                TemplateName = name,
                Description = description,
                IsActive = isActive,
                Parameters = parameters ?? ValidUpdateParameterList()
            };

        public static List<UpdateQualityTemplateParameterDto> ValidUpdateParameterList() =>
            new List<UpdateQualityTemplateParameterDto>
            {
                new UpdateQualityTemplateParameterDto
                {
                    QualityParameterId = 1,
                    SequenceNo = 1,
                    IsMandatory = true,
                    IsCritical = true,
                    InspectionMethodId = 18,
                    SampleSize = 5,
                    SampleUomId = 1,
                    IsGradeApplicable = true,
                    Remarks = "Lab tensile test",
                    IsActive = 1
                }
            };

        public static QualityTemplateDto ValidDto(
            int id = 1,
            string code = "QT-000001",
            string name = "Yarn QC - Standard Cotton") =>
            new QualityTemplateDto
            {
                Id = id,
                TemplateCode = code,
                TemplateName = name,
                Description = "Standard QC checks for cotton yarn",
                IsActive = true,
                IsDeleted = false,
                Parameters = new List<QualityTemplateParameterDto>
                {
                    new QualityTemplateParameterDto
                    {
                        Id = 1,
                        QualityParameterId = 1,
                        ParameterCode = "QP-000001",
                        ParameterName = "Tensile Strength",
                        SequenceNo = 1,
                        IsMandatory = true,
                        IsCritical = true,
                        IsGradeApplicable = true,
                        IsActive = true
                    }
                },
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        public static QualityTemplateListDto ValidListDto(int id = 1, int paramCount = 2) =>
            new QualityTemplateListDto
            {
                Id = id,
                TemplateCode = $"QT-{id:D6}",
                TemplateName = "Yarn QC - Standard Cotton",
                Description = "Standard QC checks",
                ParameterCount = paramCount,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user"
            };

        public static IReadOnlyList<QualityTemplateLookupDto> ValidLookupList() =>
            new List<QualityTemplateLookupDto>
            {
                new QualityTemplateLookupDto { Id = 1, TemplateCode = "QT-000001", TemplateName = "Yarn QC - Standard Cotton" }
            };

        public static QCManagement.Domain.Entities.QualityTemplate ValidEntity(int id = 1) =>
            new QCManagement.Domain.Entities.QualityTemplate
            {
                Id = id,
                TemplateCode = $"QT-{id:D6}",
                TemplateName = "Yarn QC - Standard Cotton",
                Description = "Standard QC checks for cotton yarn",
                IsActive = QCManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = QCManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
