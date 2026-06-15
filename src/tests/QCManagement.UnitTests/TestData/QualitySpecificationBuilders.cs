using QCManagement.Application.QualitySpecification.Commands.CreateQualitySpecification;
using QCManagement.Application.QualitySpecification.Commands.UpdateQualitySpecification;
using QCManagement.Application.QualitySpecification.Dto;

namespace QCManagement.UnitTests.TestData
{
    public static class QualitySpecificationBuilders
    {
        // Conventional FK ids used across QS unit tests (illustrative — match seed convention)
        public const int QualityTemplateId = 1;
        public const int QcTypeId = 30;                       // QP_QC_TYPE (illustrative seed id)
        public const int ApplicableLevelItemCategoryId = 17;  // QP_APPLICABLE_LEVEL: ITEM CATEGORY
        public const int ApplicableLevelItemId = 18;          // QP_APPLICABLE_LEVEL: ITEM
        public const int ValidationTypeRangeId = 11;          // QP_VALIDATION: RNG
        public const int ValidationTypeMinId = 12;            // QP_VALIDATION: MIN
        public const int ValidationTypeMaxId = 13;            // QP_VALIDATION: MAX
        public const int ValidationTypeFixedId = 14;          // QP_VALIDATION: FIX
        public const int ValidationTypePassFailId = 15;       // QP_VALIDATION: PFL
        public const int ValidationTypeListId = 16;           // QP_VALIDATION: LST
        public const int SeverityCriticalId = 19;             // QP_SEVERITY: CRT
        public const int FailureActionAcceptId = 22;          // QP_FAILURE_ACTION: ACPT
        public const int FailureActionRejectId = 26;          // QP_FAILURE_ACTION: REJECT

        public static CreateQualitySpecificationCommand ValidCreateCommand(
            string? name = "Cotton Yarn 40s — Standard Acceptance v1",
            int qualityTemplateId = QualityTemplateId,
            int qcTypeId = QcTypeId,
            int applicableLevelId = ApplicableLevelItemCategoryId,
            int? itemCategoryId = 5,
            int? itemId = null,
            DateTimeOffset? effectiveFrom = null,
            DateTimeOffset? effectiveTo = null,
            List<CreateQualitySpecificationParameterDto>? parameters = null) =>
            new CreateQualitySpecificationCommand
            {
                SpecificationName = name,
                QualityTemplateId = qualityTemplateId,
                QcTypeId = qcTypeId,
                ApplicableLevelId = applicableLevelId,
                ItemCategoryId = itemCategoryId,
                ItemId = itemId,
                Description = "Acceptance limits for 40s cotton yarn",
                EffectiveFrom = effectiveFrom ?? new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
                EffectiveTo = effectiveTo,
                Parameters = parameters ?? ValidCreateParameterList()
            };

        public static List<CreateQualitySpecificationParameterDto> ValidCreateParameterList() =>
            new List<CreateQualitySpecificationParameterDto>
            {
                new CreateQualitySpecificationParameterDto
                {
                    QualityParameterId = 1,
                    ValidationTypeId = ValidationTypeRangeId,
                    MinValue = 39.5m,
                    MaxValue = 40.5m,
                    SeverityId = SeverityCriticalId,
                    FailureActionId = FailureActionRejectId,
                    IsSamplingRequired = true,
                    Remarks = "Tested per ASTM D2256"
                },
                new CreateQualitySpecificationParameterDto
                {
                    QualityParameterId = 2,
                    ValidationTypeId = ValidationTypeListId,
                    AllowedValues = new List<string> { "A", "B", "C" },
                    SeverityId = SeverityCriticalId,
                    FailureActionId = FailureActionAcceptId
                }
            };

        public static UpdateQualitySpecificationCommand ValidUpdateCommand(
            int id = 1,
            string? name = "Updated Specification",
            int qcTypeId = QcTypeId,
            DateTimeOffset? effectiveFrom = null,
            DateTimeOffset? effectiveTo = null,
            int isActive = 1,
            List<UpdateQualitySpecificationParameterDto>? parameters = null) =>
            new UpdateQualitySpecificationCommand
            {
                Id = id,
                SpecificationName = name,
                QcTypeId = qcTypeId,
                Description = "Updated description",
                EffectiveFrom = effectiveFrom ?? new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
                EffectiveTo = effectiveTo,
                IsActive = isActive,
                Parameters = parameters ?? ValidUpdateParameterList()
            };

        public static List<UpdateQualitySpecificationParameterDto> ValidUpdateParameterList() =>
            new List<UpdateQualitySpecificationParameterDto>
            {
                new UpdateQualitySpecificationParameterDto
                {
                    Id = 1,
                    ValidationTypeId = ValidationTypeRangeId,
                    MinValue = 39.0m,
                    MaxValue = 41.0m,
                    SeverityId = SeverityCriticalId,
                    FailureActionId = FailureActionRejectId,
                    IsSamplingRequired = true,
                    Remarks = "Updated remarks",
                    IsActive = 1
                }
            };

        public static QualitySpecificationDto ValidDto(
            int id = 1,
            string code = "QS-0001",
            string name = "Cotton Yarn 40s — Standard Acceptance v1") =>
            new QualitySpecificationDto
            {
                Id = id,
                SpecificationCode = code,
                SpecificationName = name,
                QualityTemplateId = QualityTemplateId,
                QualityTemplateName = "Cotton Yarn Template",
                ApplicableLevelId = ApplicableLevelItemCategoryId,
                ApplicableLevelCode = "ITEM CATEGORY",
                ApplicableLevelName = "Item Category",
                ItemCategoryId = 5,
                ItemCategoryName = "Cotton Yarn",
                Description = "Acceptance limits",
                EffectiveFrom = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
                IsActive = true,
                IsDeleted = false,
                Parameters = new List<QualitySpecificationParameterDto>
                {
                    new QualitySpecificationParameterDto
                    {
                        Id = 1,
                        QualityParameterId = 1,
                        ParameterCode = "QP-000001",
                        ParameterName = "Tensile Strength",
                        ValidationTypeId = ValidationTypeRangeId,
                        ValidationTypeCode = "RNG",
                        ValidationTypeName = "Range",
                        MinValue = 39.5m,
                        MaxValue = 40.5m,
                        IsSamplingRequired = true,
                        IsActive = true
                    }
                },
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        public static QualitySpecificationListDto ValidListDto(int id = 1, int paramCount = 2) =>
            new QualitySpecificationListDto
            {
                Id = id,
                SpecificationCode = $"QS-{id:D4}",
                SpecificationName = "Cotton Yarn 40s — Standard Acceptance v1",
                QualityTemplateId = QualityTemplateId,
                QualityTemplateName = "Cotton Yarn Template",
                ApplicableLevelId = ApplicableLevelItemCategoryId,
                ApplicableLevelName = "Item Category",
                ItemCategoryId = 5,
                AppliesTo = "Cotton Yarn",
                EffectiveFrom = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
                ParameterCount = paramCount,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user"
            };

        public static IReadOnlyList<QualitySpecificationLookupDto> ValidLookupList() =>
            new List<QualitySpecificationLookupDto>
            {
                new QualitySpecificationLookupDto { Id = 1, SpecificationCode = "QS-0001", SpecificationName = "Cotton Yarn 40s — Standard Acceptance v1" }
            };

        public static QCManagement.Domain.Entities.QualitySpecification ValidEntity(int id = 1) =>
            new QCManagement.Domain.Entities.QualitySpecification
            {
                Id = id,
                SpecificationCode = $"QS-{id:D4}",
                SpecificationName = "Cotton Yarn 40s — Standard Acceptance v1",
                QualityTemplateId = QualityTemplateId,
                ApplicableLevelId = ApplicableLevelItemCategoryId,
                ItemCategoryId = 5,
                EffectiveFrom = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
                IsActive = QCManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = QCManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
