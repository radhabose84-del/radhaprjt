using AutoMapper;
using FAM.Application.AssetMaster.AssetAmc.Command.CreateAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Command.DeleteAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Command.UpdateAssetAmc;
using FAM.Application.Common.Mappings.AssetMaster;
using FAM.Domain.Entities.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class AssetAmcProfileTests
    {
        private readonly IMapper _mapper;

        public AssetAmcProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<AssetAmcProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_IsActive1_MapsTo_StatusActive()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var cmd = new CreateAssetAmcCommand
            {
                AssetId = 1,
                StartDate = today,
                Period = 12,
                VendorCode = "VND001",
                VendorName = "Vendor",
                CoverageType = 1,
                RenewalStatus = 1,
                IsActive = 1
            };

            var entity = _mapper.Map<AssetAmc>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new CreateAssetAmcCommand
            {
                AssetId = 1,
                IsActive = 0
            };

            var entity = _mapper.Map<AssetAmc>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void CreateCommand_To_Entity_IgnoresId()
        {
            var cmd = new CreateAssetAmcCommand { AssetId = 5, IsActive = 1 };

            var entity = _mapper.Map<AssetAmc>(cmd);

            entity.Id.Should().Be(0);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteAssetAmcCommand { Id = 9 };

            var entity = _mapper.Map<AssetAmc>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateAssetAmcCommand
            {
                VendorCode = "VND002",
                VendorName = "Updated Vendor",
                IsActive = 1
            };

            var entity = _mapper.Map<AssetAmc>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }
    }
}
