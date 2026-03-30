using AutoMapper;
using FAM.Application.AssetMaster.AssetSpecification.Commands.DeleteAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Application.Common.Mappings.AssetMaster;
using FAM.Domain.Entities.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class AssetSpecificationProfileTests
    {
        private readonly IMapper _mapper;

        public AssetSpecificationProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<AssetSpecificationProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void DeleteCommand_MapsTo_Entity_WithIsDeletedDeleted()
        {
            var cmd = new DeleteAssetSpecificationCommand { Id = 5 };

            var entity = _mapper.Map<AssetSpecifications>(cmd);

            entity.Id.Should().Be(5);
            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public void Entity_MapsTo_SpecificationDTO()
        {
            var entity = new AssetSpecifications
            {
                Id = 1,
                AssetId = 10,
                SpecificationId = 5,
                SpecificationValue = "100kg",
                IsActive = Status.Active
            };

            var dto = _mapper.Map<AssetSpecificationDTO>(entity);

            dto.AssetId.Should().Be(10);
            dto.SpecificationId.Should().Be(5);
            dto.SpecificationValue.Should().Be("100kg");
        }
    }
}
