using AutoMapper;
using InventoryManagement.Application.Common.Mappings;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.CreateItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.UpdateItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Mappings
{
    public sealed class ItemSpecificationMasterProfileTests
    {
        private readonly IMapper _mapper;

        public ItemSpecificationMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<ItemSpecificationMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateItemSpecificationMasterCommand
            {
                SpecificationCode = "SPEC001",
                SpecificationName = "Color",
                Order = 1
            };

            var entity = _mapper.Map<DomainEntities.ItemSpecificationMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateItemSpecificationMasterCommand
            {
                SpecificationCode = "SPEC001",
                SpecificationName = "Color",
                Order = 1
            };

            var entity = _mapper.Map<DomainEntities.ItemSpecificationMaster>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CreateCommand_To_Entity_CopiesProperties()
        {
            var cmd = new CreateItemSpecificationMasterCommand
            {
                SpecificationCode = "SPEC001",
                SpecificationName = "Color",
                Order = 5
            };

            var entity = _mapper.Map<DomainEntities.ItemSpecificationMaster>(cmd);

            entity.SpecificationCode.Should().Be("SPEC001");
            entity.SpecificationName.Should().Be("Color");
            entity.Order.Should().Be(5);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateItemSpecificationMasterCommand
            {
                Id = 1,
                SpecificationName = "Updated",
                Order = 2,
                IsActive = 1
            };

            var entity = _mapper.Map<DomainEntities.ItemSpecificationMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateItemSpecificationMasterCommand
            {
                Id = 1,
                SpecificationName = "Updated",
                Order = 2,
                IsActive = 0
            };

            var entity = _mapper.Map<DomainEntities.ItemSpecificationMaster>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Entity_To_Dto_MapsIsActiveAsBool()
        {
            var entity = new DomainEntities.ItemSpecificationMaster
            {
                Id = 1,
                SpecificationCode = "SPEC001",
                SpecificationName = "Color",
                Order = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var dto = _mapper.Map<ItemSpecificationMasterDto>(entity);

            dto.IsActive.Should().BeTrue();
            dto.IsDeleted.Should().BeFalse();
            dto.SpecificationCode.Should().Be("SPEC001");
            dto.Order.Should().Be(1);
        }

        [Fact]
        public void Entity_To_LookupDto_MapsBasicFields()
        {
            var entity = new DomainEntities.ItemSpecificationMaster
            {
                Id = 5,
                SpecificationCode = "SPEC005",
                SpecificationName = "Size",
                Order = 2
            };

            var dto = _mapper.Map<ItemSpecificationMasterLookupDto>(entity);

            dto.Id.Should().Be(5);
            dto.SpecificationCode.Should().Be("SPEC005");
            dto.SpecificationName.Should().Be("Size");
            dto.Order.Should().Be(2);
        }
    }
}
