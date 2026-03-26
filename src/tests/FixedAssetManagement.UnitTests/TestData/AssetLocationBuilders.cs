using FAM.Application.AssetLocation.Commands.CreateAssetLocation;
using FAM.Application.AssetLocation.Queries.GetAssetLocation;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetLocationTestBuilders
    {
        public static CreateAssetLocationCommand ValidCreateCommand(
            int assetId = 1,
            int unitId = 1,
            int departmentId = 2,
            int locationId = 3,
            int subLocationId = 4) =>
            new CreateAssetLocationCommand
            {
                AssetId = assetId,
                UnitId = unitId,
                DepartmentId = departmentId,
                LocationId = locationId,
                SubLocationId = subLocationId,
                CustodianId = 5,
                UserID = 10
            };

        public static AssetLocationDto ValidDto(int id = 1) =>
            new AssetLocationDto
            {
                Id = id,
                AssetId = 1,
                UnitId = 1,
                DepartmentId = 2,
                LocationId = 3,
                SubLocationId = 4
            };

        public static FAM.Domain.Entities.AssetMaster.AssetLocation ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.AssetMaster.AssetLocation
            {
                Id = id,
                AssetId = 1,
                UnitId = 1,
                DepartmentId = 2,
                LocationId = 3,
                SubLocationId = 4,
                CustodianId = 5,
                UserID = 10
            };
    }
}
