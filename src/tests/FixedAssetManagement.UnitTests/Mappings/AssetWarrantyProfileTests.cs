using AutoMapper;
using FAM.Application.AssetMaster.AssetWarranty.Commands.CreateAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.Mappings.AssetMaster;
using FAM.Domain.Entities.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class AssetWarrantyProfileTests
    {
        private readonly IMapper _mapper;

        public AssetWarrantyProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<AssetWarrantyProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_MapsTo_Entity_WithIsActiveAndIsDeleted()
        {
            var cmd = new CreateAssetWarrantyCommand
            {
                AssetId = 10,
                WarrantyType = 1,
                Description = "Standard Warranty"
            };

            var entity = _mapper.Map<AssetWarranties>(cmd);

            entity.AssetId.Should().Be(10);
            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Entity_MapsTo_Dto()
        {
            var entity = new AssetWarranties
            {
                Id = 1,
                AssetId = 10,
                WarrantyType = 1,
                Description = "Standard Warranty",
                IsActive = Status.Active
            };

            var dto = _mapper.Map<AssetWarrantyDTO>(entity);

            dto.Id.Should().Be(1);
            dto.AssetId.Should().Be(10);
            dto.Description.Should().Be("Standard Warranty");
        }
    }
}
