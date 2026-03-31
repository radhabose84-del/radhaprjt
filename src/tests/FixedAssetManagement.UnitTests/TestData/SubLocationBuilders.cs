using FAM.Application.Location.Command.DeleteAubLocation;
using FAM.Application.Location.Command.UpdateSubLocation;
using FAM.Application.SubLocation.Command.CreateSubLocation;
using FAM.Application.SubLocation.Queries.GetSubLocations;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class SubLocationBuilders
    {
        public static CreateSubLocationCommand ValidCreateCommand(
            string? code = "SL001",
            string? subLocationName = "Test SubLocation",
            int unitId = 1,
            int departmentId = 1,
            int locationId = 1) =>
            new CreateSubLocationCommand
            {
                Code = code,
                SubLocationName = subLocationName,
                Description = "Test description",
                UnitId = unitId,
                DepartmentId = departmentId,
                LocationId = locationId
            };

        public static UpdateSubLocationCommand ValidUpdateCommand(
            int id = 1,
            string? subLocationName = "Updated SubLocation",
            int unitId = 1,
            int departmentId = 1,
            int locationId = 1) =>
            new UpdateSubLocationCommand
            {
                Id = id,
                Code = "SL001",
                SubLocationName = subLocationName,
                Description = "Updated description",
                UnitId = unitId,
                DepartmentId = departmentId,
                LocationId = locationId,
                IsActive = 1
            };

        public static DeleteSubLocationCommand ValidDeleteCommand(int id = 1) =>
            new DeleteSubLocationCommand { Id = id };

        public static SubLocationDto ValidDto(int id = 1) =>
            new SubLocationDto
            {
                Id = id,
                Code = "SL001",
                SubLocationName = "Test SubLocation",
                Description = "Test description",
                UnitId = 1,
                DepartmentId = 1,
                DepartmentName = "Test Department",
                LocationId = 1,
                IsActive = Status.Active
            };

        public static FAM.Domain.Entities.SubLocation ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.SubLocation
            {
                Id = id,
                Code = "SL001",
                SubLocationName = "Test SubLocation",
                Description = "Test description",
                UnitId = 1,
                DepartmentId = 1,
                LocationId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
