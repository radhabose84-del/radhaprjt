using QCManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using QCManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using QCManagement.Application.MiscMaster.Dto;

namespace QCManagement.UnitTests.TestData
{
    public static class MiscMasterBuilders
    {
        public static CreateMiscMasterCommand ValidCreateCommand(
            int miscTypeId = 1,
            string? code = "PHY",
            string? description = "Physical") =>
            new CreateMiscMasterCommand
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description
            };

        public static UpdateMiscMasterCommand ValidUpdateCommand(
            int id = 1,
            string? description = "Updated Physical",
            int sortOrder = 1,
            int isActive = 1) =>
            new UpdateMiscMasterCommand
            {
                Id = id,
                Description = description,
                SortOrder = sortOrder,
                IsActive = isActive
            };

        public static MiscMasterDto ValidDto(
            int id = 1,
            int miscTypeId = 1,
            string miscTypeCode = "QP_GROUP",
            string code = "PHY",
            string description = "Physical") =>
            new MiscMasterDto
            {
                Id = id,
                MiscTypeId = miscTypeId,
                MiscTypeCode = miscTypeCode,
                MiscTypeDescription = "Quality Parameter Group",
                Code = code,
                Description = description,
                SortOrder = 1,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        public static IReadOnlyList<MiscMasterLookupDto> ValidLookupList() =>
            new List<MiscMasterLookupDto>
            {
                new MiscMasterLookupDto { Id = 1, MiscTypeId = 1, MiscTypeCode = "QP_GROUP", Code = "PHY", Description = "Physical" },
                new MiscMasterLookupDto { Id = 2, MiscTypeId = 1, MiscTypeCode = "QP_GROUP", Code = "CHM", Description = "Chemical" }
            };

        public static QCManagement.Domain.Entities.MiscMaster ValidEntity(int id = 1) =>
            new QCManagement.Domain.Entities.MiscMaster
            {
                Id = id,
                MiscTypeId = 1,
                Code = "PHY",
                Description = "Physical",
                SortOrder = 1,
                IsActive = QCManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = QCManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
