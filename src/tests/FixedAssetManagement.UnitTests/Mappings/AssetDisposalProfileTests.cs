using AutoMapper;
using FAM.Application.AssetMaster.AssetDisposal.Command.CreateAssetDisposal;
using FAM.Application.AssetMaster.AssetDisposal.Command.UpdateAssetDisposal;
using FAM.Application.Common.Mappings.AssetMaster;
using FAM.Domain.Entities.AssetMaster;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class AssetDisposalProfileTests
    {
        private readonly IMapper _mapper;

        public AssetDisposalProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<AssetDisposalProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_MapsAssetId()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var cmd = new CreateAssetDisposalCommand
            {
                AssetId = 10,
                AssetPurchaseId = 5,
                DisposalDate = today,
                DisposalType = 2,
                DisposalReason = "Obsolete",
                DisposalAmount = 1000m
            };

            var entity = _mapper.Map<AssetDisposal>(cmd);

            entity.AssetId.Should().Be(10);
            entity.DisposalDate.Should().Be(today);
        }

        [Fact]
        public void CreateCommand_To_Entity_IgnoresId()
        {
            var cmd = new CreateAssetDisposalCommand
            {
                AssetId = 5,
                AssetPurchaseId = 3,
                DisposalDate = DateOnly.FromDateTime(DateTime.Today)
            };

            var entity = _mapper.Map<AssetDisposal>(cmd);

            entity.Id.Should().Be(0);
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsDisposalReason()
        {
            var cmd = new UpdateAssetDisposalCommand
            {
                DisposalReason = "Written Off",
                DisposalAmount = 500m
            };

            var entity = _mapper.Map<AssetDisposal>(cmd);

            entity.DisposalReason.Should().Be("Written Off");
        }

        [Fact]
        public void UpdateCommand_To_Entity_IgnoresAssetId()
        {
            var cmd = new UpdateAssetDisposalCommand
            {
                DisposalReason = "Sold"
            };

            var entity = _mapper.Map<AssetDisposal>(cmd);

            entity.AssetId.Should().Be(0);
        }
    }
}
