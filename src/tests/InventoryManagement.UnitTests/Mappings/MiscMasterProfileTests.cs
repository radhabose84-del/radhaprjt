using AutoMapper;
using InventoryManagement.Application.Common.Mappings;
using InventoryManagement.Application.MiscMaster.Command.CreateMiscMaster;
using InventoryManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using InventoryManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Mappings
{
    public sealed class MiscMasterProfileTests
    {
        private readonly IMapper _mapper;

        public MiscMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<MiscMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateMiscMasterCommand { Code = "MSC001", Description = "Test", MiscTypeId = 1 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.MiscMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateMiscMasterCommand { Code = "MSC001", Description = "Test", MiscTypeId = 1 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.MiscMaster>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateMiscMasterCommand { Id = 1, Description = "Updated", IsActive = 1 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.MiscMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateMiscMasterCommand { Id = 1, Description = "Updated", IsActive = 0 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.MiscMaster>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteMiscMasterCommand { Id = 5 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.MiscMaster>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
