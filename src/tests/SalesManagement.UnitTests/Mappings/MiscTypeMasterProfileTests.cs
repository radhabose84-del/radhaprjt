using AutoMapper;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Mappings
{
    public sealed class MiscTypeMasterProfileTests
    {
        private readonly IMapper _mapper;

        public MiscTypeMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MiscTypeMasterProfile>());
            _mapper = config.CreateMapper();
        }


        [Fact]
        public void CreateCommand_To_Entity_MapsFields()
        {
            var command = new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = "MT001",
                Description = "Test Type"
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MiscTypeMaster>(command);

            entity.MiscTypeCode.Should().Be("MT001");
            entity.Description.Should().Be("Test Type");
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var command = new CreateMiscTypeMasterCommand { MiscTypeCode = "MT001" };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MiscTypeMaster>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var command = new CreateMiscTypeMasterCommand { MiscTypeCode = "MT001" };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MiscTypeMaster>(command);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateMiscTypeMasterCommand { Id = 1, Description = "Updated", IsActive = 1 };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MiscTypeMaster>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateMiscTypeMasterCommand { Id = 1, Description = "Updated", IsActive = 0 };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MiscTypeMaster>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsFields()
        {
            var command = new UpdateMiscTypeMasterCommand
            {
                Id = 5,
                Description = "Updated Desc",
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MiscTypeMaster>(command);

            entity.Id.Should().Be(5);
            entity.Description.Should().Be("Updated Desc");
        }
    }
}
