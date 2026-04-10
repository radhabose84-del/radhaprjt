using AutoMapper;
using InventoryManagement.Application.Common.Mappings;
using InventoryManagement.Application.ItemSpecificationValue.Commands.CreateItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Commands.UpdateItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Mappings
{
    public sealed class ItemSpecificationValueProfileTests
    {
        private readonly IMapper _mapper;

        public ItemSpecificationValueProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<ItemSpecificationValueProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateItemSpecificationValueCommand
            {
                SpecificationMasterId = 1,
                SpecificationValue = "Red"
            };

            var entity = _mapper.Map<DomainEntities.ItemSpecificationValue>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateItemSpecificationValueCommand
            {
                SpecificationMasterId = 1,
                SpecificationValue = "Red"
            };

            var entity = _mapper.Map<DomainEntities.ItemSpecificationValue>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CreateCommand_To_Entity_CopiesProperties()
        {
            var cmd = new CreateItemSpecificationValueCommand
            {
                SpecificationMasterId = 5,
                SpecificationValue = "Blue"
            };

            var entity = _mapper.Map<DomainEntities.ItemSpecificationValue>(cmd);

            entity.SpecificationMasterId.Should().Be(5);
            entity.SpecificationValue.Should().Be("Blue");
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateItemSpecificationValueCommand
            {
                Id = 1,
                SpecificationMasterId = 1,
                SpecificationValue = "Updated Red",
                IsActive = 1
            };

            var entity = _mapper.Map<DomainEntities.ItemSpecificationValue>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateItemSpecificationValueCommand
            {
                Id = 1,
                SpecificationMasterId = 1,
                SpecificationValue = "Updated Red",
                IsActive = 0
            };

            var entity = _mapper.Map<DomainEntities.ItemSpecificationValue>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Entity_To_Dto_MapsIsActiveAsBool_AndIgnoresMasterName()
        {
            var entity = new DomainEntities.ItemSpecificationValue
            {
                Id = 1,
                SpecificationMasterId = 2,
                SpecificationValue = "Red",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var dto = _mapper.Map<ItemSpecificationValueDto>(entity);

            dto.IsActive.Should().BeTrue();
            dto.IsDeleted.Should().BeFalse();
            dto.SpecificationMasterId.Should().Be(2);
            dto.SpecificationValue.Should().Be("Red");
            dto.SpecificationMasterName.Should().BeNull();
        }

        [Fact]
        public void Entity_To_LookupDto_MapsBasicFields()
        {
            var entity = new DomainEntities.ItemSpecificationValue
            {
                Id = 10,
                SpecificationMasterId = 3,
                SpecificationValue = "Green"
            };

            var dto = _mapper.Map<ItemSpecificationValueLookupDto>(entity);

            dto.Id.Should().Be(10);
            dto.SpecificationMasterId.Should().Be(3);
            dto.SpecificationValue.Should().Be("Green");
        }
    }
}
