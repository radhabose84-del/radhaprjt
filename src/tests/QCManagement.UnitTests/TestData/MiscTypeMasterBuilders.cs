using QCManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using QCManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using QCManagement.Application.MiscTypeMaster.Dto;

namespace QCManagement.UnitTests.TestData
{
    public static class MiscTypeMasterBuilders
    {
        public static CreateMiscTypeMasterCommand ValidCreateCommand(
            string? code = "QPGROUP",
            string? description = "Quality Parameter Group") =>
            new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = code,
                Description = description
            };

        public static UpdateMiscTypeMasterCommand ValidUpdateCommand(
            int id = 1,
            string? description = "Updated Quality Parameter Group",
            int isActive = 1) =>
            new UpdateMiscTypeMasterCommand
            {
                Id = id,
                Description = description,
                IsActive = isActive
            };

        public static MiscTypeMasterDto ValidDto(
            int id = 1,
            string code = "QP_GROUP",
            string description = "Quality Parameter Group") =>
            new MiscTypeMasterDto
            {
                Id = id,
                MiscTypeCode = code,
                Description = description,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        public static IReadOnlyList<MiscTypeMasterLookupDto> ValidLookupList() =>
            new List<MiscTypeMasterLookupDto>
            {
                new MiscTypeMasterLookupDto { Id = 1, MiscTypeCode = "QP_GROUP",    Description = "Quality Parameter Group" },
                new MiscTypeMasterLookupDto { Id = 2, MiscTypeCode = "QP_DATATYPE", Description = "Quality Parameter Data Type" }
            };

        public static QCManagement.Domain.Entities.MiscTypeMaster ValidEntity(int id = 1) =>
            new QCManagement.Domain.Entities.MiscTypeMaster
            {
                Id = id,
                MiscTypeCode = "QP_GROUP",
                Description = "Quality Parameter Group",
                IsActive = QCManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = QCManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
