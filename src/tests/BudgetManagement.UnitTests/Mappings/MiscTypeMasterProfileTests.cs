using AutoMapper;
using BudgetManagement.Application.Common.Mappings;
using BudgetManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using BudgetManagement.UnitTests.TestData;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.Mappings
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
        public void CreateCommand_MapsToEntity_WithActiveAndNotDeleted()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.MiscTypeMaster>(command);

            entity.MiscTypeCode.Should().Be(command.MiscTypeCode);
            entity.Description.Should().Be(command.Description);
            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsToActive()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(isActive: 1);

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.MiscTypeMaster>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsToInactive()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(isActive: 0);

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.MiscTypeMaster>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_MapsToEntity_WithDeleted()
        {
            var command = MiscTypeMasterBuilders.ValidDeleteCommand();

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.MiscTypeMaster>(command);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
