using AutoMapper;
using InventoryManagement.Application.Common.Mappings;
using InventoryManagement.Application.UsageType.Commands.CreateUsageType;
using InventoryManagement.Application.UsageType.Commands.DeleteUsageType;
using InventoryManagement.Application.UsageType.Commands.UpdateUsageType;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Mappings
{
    public sealed class UsageTypeProfileTests
    {
        private readonly IMapper _mapper;

        public UsageTypeProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<UsageTypeProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateUsageTypeCommand
            {
                UsageTypeCode = "UTY001",
                UsageTypeName = "Test UsageType"
            };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UsageType>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateUsageTypeCommand
            {
                UsageTypeCode = "UTY001",
                UsageTypeName = "Test UsageType"
            };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UsageType>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateUsageTypeCommand { Id = 1, UsageTypeName = "Updated", IsActive = 1 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UsageType>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateUsageTypeCommand { Id = 1, UsageTypeName = "Updated", IsActive = 0 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UsageType>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteUsageTypeCommand(3);

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UsageType>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
