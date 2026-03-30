using AutoMapper;
using BudgetManagement.Application.Common.Mappings;
using BudgetManagement.UnitTests.TestData;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.Mappings
{
    public sealed class BudgetGroupProfileTests
    {
        private readonly IMapper _mapper;

        public BudgetGroupProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<BudgetGroupMappingProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_MapsToEntity_WithActiveAndNotDeleted()
        {
            var command = BudgetGroupBuilders.ValidCreateCommand();

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.BudgetGroup>(command);

            entity.Name.Should().Be(command.Name);
            entity.UnitId.Should().Be(command.UnitId);
            entity.DepartmentId.Should().Be(command.DepartmentId);
            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActiveTrue_MapsToActive()
        {
            var command = BudgetGroupBuilders.ValidUpdateCommand(isActive: true);

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.BudgetGroup>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActiveFalse_MapsToInactive()
        {
            var command = BudgetGroupBuilders.ValidUpdateCommand(isActive: false);

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.BudgetGroup>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_MapsToEntity_WithDeleted()
        {
            var command = BudgetGroupBuilders.ValidDeleteCommand();

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.BudgetGroup>(command);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
