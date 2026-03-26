namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class AssetLocationEntityTests
    {
        [Fact]
        public void AssetLocation_Properties_ShouldBeAssignable()
        {
            var entity = new FAM.Domain.Entities.AssetMaster.AssetLocation
            {
                Id = 1,
                AssetId = 10,
                UnitId = 2,
                DepartmentId = 3,
                LocationId = 4,
                SubLocationId = 5,
                CustodianId = 6,
                UserID = 100
            };

            entity.Id.Should().Be(1);
            entity.AssetId.Should().Be(10);
            entity.UnitId.Should().Be(2);
            entity.DepartmentId.Should().Be(3);
            entity.LocationId.Should().Be(4);
            entity.SubLocationId.Should().Be(5);
            entity.CustodianId.Should().Be(6);
            entity.UserID.Should().Be(100);
        }

        [Fact]
        public void AssetLocation_DefaultValues_AreCorrect()
        {
            var entity = new FAM.Domain.Entities.AssetMaster.AssetLocation();

            entity.Id.Should().Be(0);
            entity.AssetId.Should().Be(0);
        }
    }
}
