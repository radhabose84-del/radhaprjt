using AutoMapper;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using SalesManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Mappings
{
    public sealed class MiscMasterProfileTests
    {
        private readonly IMapper _mapper;

        public MiscMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MiscMasterProfile>());
            _mapper = config.CreateMapper();
        }


        [Fact]
        public void CreateCommand_To_Entity_MapsFields()
        {
            var command = new CreateMiscMasterCommand
            {
                MiscTypeId = 1,
                Code = "MISC001",
                Description = "Test Misc"
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MiscMaster>(command);

            entity.MiscTypeId.Should().Be(1);
            entity.Code.Should().Be("MISC001");
            entity.Description.Should().Be("Test Misc");
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var command = new CreateMiscMasterCommand { MiscTypeId = 1, Code = "MISC001" };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MiscMaster>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var command = new CreateMiscMasterCommand { MiscTypeId = 1, Code = "MISC001" };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MiscMaster>(command);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateMiscMasterCommand { Id = 1, Description = "Updated", IsActive = 1 };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MiscMaster>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateMiscMasterCommand { Id = 1, Description = "Updated", IsActive = 0 };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MiscMaster>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsFields()
        {
            var command = new UpdateMiscMasterCommand
            {
                Id = 5,
                Description = "Updated Desc",
                SortOrder = 10,
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MiscMaster>(command);

            entity.Id.Should().Be(5);
            entity.Description.Should().Be("Updated Desc");
            entity.SortOrder.Should().Be(10);
        }
    }
}
