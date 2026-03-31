using AutoMapper;
using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.CreateAssetAdditionalCost;
using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.UpdateAssetAdditionalCost;
using FAM.Application.Common.Mappings.AssetMaster;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class AssetAdditionalCostProfileTests
    {
        private readonly IMapper _mapper;

        public AssetAdditionalCostProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<AssetAdditionalCostProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_MapsTo_Entity_WithCorrectFields()
        {
            var cmd = new CreateAssetAdditionalCostCommand
            {
                AssetId = 5,
                AssetSourceId = 2,
                Amount = 3000m,
                JournalNo = "JNL001",
                CostType = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>(cmd);

            entity.AssetId.Should().Be(5);
            entity.AssetSourceId.Should().Be(2);
            entity.Amount.Should().Be(3000m);
            entity.JournalNo.Should().Be("JNL001");
        }

        [Fact]
        public void UpdateCommand_MapsTo_Entity_WithCorrectFields()
        {
            var cmd = new UpdateAssetAdditionalCostCommand
            {
                Id = 1,
                Amount = 4000m,
                JournalNo = "JNL002",
                CostType = 2
            };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>(cmd);

            entity.Amount.Should().Be(4000m);
            entity.JournalNo.Should().Be("JNL002");
        }
    }
}
