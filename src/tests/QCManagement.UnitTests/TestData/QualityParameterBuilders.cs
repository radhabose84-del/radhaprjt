using QCManagement.Application.QualityParameter.Commands.CreateQualityParameter;
using QCManagement.Application.QualityParameter.Commands.UpdateQualityParameter;
using QCManagement.Application.QualityParameter.Dto;

namespace QCManagement.UnitTests.TestData
{
    public static class QualityParameterBuilders
    {
        public static CreateQualityParameterCommand ValidCreateCommand(
            string? name = "Yarn Tensile Strength",
            int parameterGroupId = 1,
            int dataTypeId = 2,
            int? unitId = 12,
            int validationTypeId = 3,
            string? description = "Breaking strength of yarn") =>
            new CreateQualityParameterCommand
            {
                ParameterName = name,
                ParameterGroupId = parameterGroupId,
                DataTypeId = dataTypeId,
                UnitId = unitId,
                ValidationTypeId = validationTypeId,
                Description = description
            };

        public static UpdateQualityParameterCommand ValidUpdateCommand(
            int id = 1,
            string? name = "Updated Tensile Strength",
            int parameterGroupId = 1,
            int? unitId = 12,
            string? description = "Updated description",
            int isActive = 1) =>
            new UpdateQualityParameterCommand
            {
                Id = id,
                ParameterName = name,
                ParameterGroupId = parameterGroupId,
                UnitId = unitId,
                Description = description,
                IsActive = isActive
            };

        public static QualityParameterDto ValidDto(
            int id = 1,
            string code = "QP-000001",
            string name = "Yarn Tensile Strength",
            int parameterGroupId = 1,
            int dataTypeId = 2,
            int? unitId = 12,
            int validationTypeId = 3) =>
            new QualityParameterDto
            {
                Id = id,
                ParameterCode = code,
                ParameterName = name,
                ParameterGroupId = parameterGroupId,
                ParameterGroupCode = "MEC",
                ParameterGroupName = "Mechanical",
                DataTypeId = dataTypeId,
                DataTypeCode = "DEC",
                DataTypeName = "Decimal",
                UnitId = unitId,
                ValidationTypeId = validationTypeId,
                ValidationTypeCode = "RNG",
                ValidationTypeName = "Range",
                Description = "Breaking strength of yarn",
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        public static IReadOnlyList<QualityParameterLookupDto> ValidLookupList() =>
            new List<QualityParameterLookupDto>
            {
                new QualityParameterLookupDto { Id = 1, ParameterCode = "QP-000001", ParameterName = "Yarn Tensile Strength" },
                new QualityParameterLookupDto { Id = 2, ParameterCode = "QP-000002", ParameterName = "Moisture Content" }
            };

        public static QCManagement.Domain.Entities.QualityParameter ValidEntity(int id = 1) =>
            new QCManagement.Domain.Entities.QualityParameter
            {
                Id = id,
                ParameterCode = "QP-000001",
                ParameterName = "Yarn Tensile Strength",
                ParameterGroupId = 1,
                DataTypeId = 2,
                UnitId = 12,
                ValidationTypeId = 3,
                Description = "Breaking strength of yarn",
                IsActive = QCManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = QCManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
