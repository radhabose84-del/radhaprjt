using AutoMapper;
using FAM.Application.AssetMaster.AssetPurchase.Commands.CreateAssetPurchaseDetails;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchase;
using FAM.Application.Common.Mappings.AssetPurchase;
using FAM.Domain.Entities.AssetPurchase;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class AssetPurchaseProfileTests
    {
        private readonly IMapper _mapper;

        public AssetPurchaseProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<AssetPurchaseProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_MapsTo_AssetPurchaseDetails()
        {
            var command = new CreateAssetPurchaseDetailCommand
            {
                AssetId = 1,
                AssetSourceId = 2,
                ItemName = "Test Item",
                ItemCode = "ITM001",
                PurchaseValue = 50000m
            };

            var entity = _mapper.Map<AssetPurchaseDetails>(command);

            entity.Should().NotBeNull();
            entity.AssetId.Should().Be(1);
            entity.AssetSourceId.Should().Be(2);
            entity.ItemName.Should().Be("Test Item");
            entity.PurchaseValue.Should().Be(50000m);
        }

        [Fact]
        public void AssetPurchaseDetails_MapsTo_Dto()
        {
            var entity = new AssetPurchaseDetails
            {
                Id = 1,
                AssetId = 1,
                AssetSourceId = 1,
                ItemName = "Test Item",
                PurchaseValue = 50000m
            };

            var dto = _mapper.Map<AssetPurchaseDetailsDto>(entity);

            dto.Should().NotBeNull();
            dto.Id.Should().Be(1);
            dto.ItemName.Should().Be("Test Item");
            dto.PurchaseValue.Should().Be(50000m);
        }
    }
}
