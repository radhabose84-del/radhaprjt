using AutoMapper;
using FAM.Application.AssetLocation.Commands.CreateAssetLocation;
using FAM.Application.AssetLocation.Queries.GetAssetLocation;
using FAM.Application.Common.Mappings;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class AssetLocationProfileTests
    {
        private readonly IMapper _mapper;

        public AssetLocationProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<AssetLocationProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_MapsTo_Entity_WithCorrectFields()
        {
            var cmd = new CreateAssetLocationCommand
            {
                AssetId = 10,
                UnitId = 2,
                DepartmentId = 3,
                LocationId = 4,
                SubLocationId = 5,
                CustodianId = 6,
                UserID = 100
            };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetMaster.AssetLocation>(cmd);

            entity.AssetId.Should().Be(10);
            entity.UnitId.Should().Be(2);
            entity.DepartmentId.Should().Be(3);
            entity.LocationId.Should().Be(4);
        }

        [Fact]
        public void Entity_MapsTo_Dto_WithCorrectFields()
        {
            var entity = new FAM.Domain.Entities.AssetMaster.AssetLocation
            {
                Id = 1,
                AssetId = 10,
                UnitId = 2,
                DepartmentId = 3,
                LocationId = 4,
                SubLocationId = 5
            };

            var dto = _mapper.Map<AssetLocationDto>(entity);

            dto.Id.Should().Be(1);
            dto.AssetId.Should().Be(10);
            dto.UnitId.Should().Be(2);
        }
    }
}
