using AutoMapper;
using BudgetManagement.Application.Common.Mappings;
using BudgetManagement.Application.MiscMaster.Command.CreateMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using BudgetManagement.UnitTests.TestData;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.Mappings
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
        public void CreateCommand_MapsToEntity_WithActiveAndNotDeleted()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.MiscMaster>(command);

            entity.Code.Should().Be(command.Code);
            entity.Description.Should().Be(command.Description);
            entity.MiscTypeId.Should().Be(command.MiscTypeId);
            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsToActive()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(isActive: 1);

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.MiscMaster>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsToInactive()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(isActive: 0);

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.MiscMaster>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_MapsToEntity_WithDeleted()
        {
            var command = MiscMasterBuilders.ValidDeleteCommand();

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.MiscMaster>(command);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
