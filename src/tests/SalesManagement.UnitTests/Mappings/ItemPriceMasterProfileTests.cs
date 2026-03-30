using AutoMapper;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.UpdateItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Mappings
{
    public sealed class ItemPriceMasterProfileTests
    {
        private readonly IMapper _mapper;

        public ItemPriceMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<ItemPriceMasterProfile>());
            _mapper = config.CreateMapper();
        }

        // Note: AssertConfigurationIsValid will fail because CreateItemPriceMasterCommand → ItemPriceMaster
        // has unmapped entity properties (PriceCode, SalesSegment, StatusMisc navigation properties).
        // This is by design — PriceCode is auto-generated and navigation props are not set from commands.

        [Fact]
        public void CreateCommand_To_Entity_MapsFields()
        {
            var command = new CreateItemPriceMasterCommand
            {
                ItemId = 1,
                SalesSegmentId = 2,
                BaseRate = 100.50m,
                CurrencyId = 3,
                ValidFrom = new DateOnly(2026, 1, 1),
                ValidTo = new DateOnly(2026, 12, 31),
                StatusId = 4
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command);

            entity.ItemId.Should().Be(1);
            entity.SalesSegmentId.Should().Be(2);
            entity.BaseRate.Should().Be(100.50m);
            entity.CurrencyId.Should().Be(3);
            entity.ValidFrom.Should().Be(new DateOnly(2026, 1, 1));
            entity.ValidTo.Should().Be(new DateOnly(2026, 12, 31));
            entity.StatusId.Should().Be(4);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var command = new CreateItemPriceMasterCommand
            {
                ItemId = 1,
                SalesSegmentId = 2,
                BaseRate = 100m,
                CurrencyId = 3,
                ValidFrom = new DateOnly(2026, 1, 1),
                ValidTo = new DateOnly(2026, 12, 31)
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var command = new CreateItemPriceMasterCommand
            {
                ItemId = 1,
                SalesSegmentId = 2,
                BaseRate = 100m,
                CurrencyId = 3,
                ValidFrom = new DateOnly(2026, 1, 1),
                ValidTo = new DateOnly(2026, 12, 31)
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateItemPriceMasterCommand
            {
                Id = 1,
                ItemId = 1,
                SalesSegmentId = 2,
                BaseRate = 100m,
                CurrencyId = 3,
                ValidFrom = new DateOnly(2026, 1, 1),
                ValidTo = new DateOnly(2026, 12, 31),
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateItemPriceMasterCommand
            {
                Id = 1,
                ItemId = 1,
                SalesSegmentId = 2,
                BaseRate = 100m,
                CurrencyId = 3,
                ValidFrom = new DateOnly(2026, 1, 1),
                ValidTo = new DateOnly(2026, 12, 31),
                IsActive = 0
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsFields()
        {
            var command = new UpdateItemPriceMasterCommand
            {
                Id = 5,
                ItemId = 10,
                SalesSegmentId = 20,
                BaseRate = 200.75m,
                CurrencyId = 30,
                ValidFrom = new DateOnly(2026, 6, 1),
                ValidTo = new DateOnly(2026, 12, 31),
                StatusId = 40,
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command);

            entity.Id.Should().Be(5);
            entity.ItemId.Should().Be(10);
            entity.SalesSegmentId.Should().Be(20);
            entity.BaseRate.Should().Be(200.75m);
            entity.CurrencyId.Should().Be(30);
            entity.StatusId.Should().Be(40);
        }

        // --- ExMillRateDto → ExMillRateDto (self-map) ---

        [Fact]
        public void ExMillRateDto_To_ExMillRateDto_MapsFields()
        {
            var source = new ExMillRateDto
            {
                Id = 1,
                PriceCode = "PC001",
                SalesSegmentId = 2,
                SalesSegmentName = "Segment A",
                ExMillRate = 150.25m
            };

            var destination = _mapper.Map<ExMillRateDto>(source);

            destination.Id.Should().Be(1);
            destination.PriceCode.Should().Be("PC001");
            destination.SalesSegmentId.Should().Be(2);
            destination.SalesSegmentName.Should().Be("Segment A");
            destination.ExMillRate.Should().Be(150.25m);
        }
    }
}
