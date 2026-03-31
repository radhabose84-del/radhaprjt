using FAM.Application.Location.Command.CreateLocation;
using FAM.Application.Location.Command.DeleteLocation;
using FAM.Application.Location.Command.UpdateLocation;
using FAM.Application.Location.Queries.GetLocations;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class LocationBuilders
    {
        public static CreateLocationCommand ValidCreateCommand(
            string? code = "LOC001",
            string? locationName = "Test Location",
            int unitId = 1,
            int departmentId = 1) =>
            new CreateLocationCommand
            {
                Code = code,
                LocationName = locationName,
                Description = "Test description",
                SortOrder = 1,
                UnitId = unitId,
                DepartmentId = departmentId
            };

        public static UpdateLocationCommand ValidUpdateCommand(
            int id = 1,
            string? locationName = "Updated Location",
            int unitId = 1,
            int departmentId = 1) =>
            new UpdateLocationCommand
            {
                Id = id,
                Code = "LOC001",
                LocationName = locationName,
                Description = "Updated description",
                SortOrder = 1,
                UnitId = unitId,
                DepartmentId = departmentId,
                IsActive = 1
            };

        public static DeleteLocationCommand ValidDeleteCommand(int id = 1) =>
            new DeleteLocationCommand { Id = id };

        public static LocationDto ValidDto(int id = 1) =>
            new LocationDto
            {
                Id = id,
                Code = "LOC001",
                LocationName = "Test Location",
                Description = "Test description",
                SortOrder = 1,
                UnitId = 1,
                DepartmentId = 1,
                DepartmentName = "Test Department",
                IsActive = Status.Active
            };

        public static FAM.Domain.Entities.Location ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.Location
            {
                Id = id,
                Code = "LOC001",
                LocationName = "Test Location",
                Description = "Test description",
                SortOrder = 1,
                UnitId = 1,
                DepartmentId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
