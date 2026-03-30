using AutoMapper;
using UserManagement.Application.Common.Mappings;
using UserManagement.Application.State.Commands.CreateState;
using UserManagement.Application.State.Commands.DeleteState;
using UserManagement.Application.State.Commands.UpdateState;
using UserManagement.Application.State.Queries.GetStates;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Mappings
{
    public sealed class StateProfileTests
    {
        private readonly IMapper _mapper;

        public StateProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<StateProfile>());
            // Do not call AssertConfigurationIsValid() because the StateProfile
            // intentionally leaves audit fields (CreatedBy, CreatedAt, etc.) unmapped
            // since they are auto-populated by ApplicationDbContext.SaveChangesAsync().
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_Maps_To_Entity_With_Active_And_NotDeleted()
        {
            // Arrange
            var command = new CreateStateCommand
            {
                StateCode = "KA",
                StateName = "Karnataka",
                CountryId = 1
            };

            // Act
            var entity = _mapper.Map<UserManagement.Domain.Entities.States>(command);

            // Assert
            entity.StateCode.Should().Be("KA");
            entity.StateName.Should().Be("Karnataka");
            entity.CountryId.Should().Be(1);
            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
            entity.Id.Should().Be(0);
        }

        [Fact]
        public void UpdateCommand_Maps_IsActive_Correctly()
        {
            // Arrange - IsActive = 1 -> Active
            var commandActive = new UpdateStateCommand
            {
                Id = 1,
                StateCode = "KA",
                StateName = "Karnataka",
                CountryId = 1,
                IsActive = 1
            };

            // Act
            var entityActive = _mapper.Map<UserManagement.Domain.Entities.States>(commandActive);

            // Assert
            entityActive.StateName.Should().Be("Karnataka");
            entityActive.IsActive.Should().Be(Status.Active);

            // Arrange - IsActive = 0 -> Inactive
            var commandInactive = new UpdateStateCommand
            {
                Id = 2,
                StateCode = "TN",
                StateName = "Tamil Nadu",
                CountryId = 1,
                IsActive = 0
            };

            // Act
            var entityInactive = _mapper.Map<UserManagement.Domain.Entities.States>(commandInactive);

            // Assert
            entityInactive.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_Maps_IsDeleted_To_Deleted()
        {
            // Arrange
            var command = new DeleteStateCommand { Id = 5 };

            // Act
            var entity = _mapper.Map<UserManagement.Domain.Entities.States>(command);

            // Assert
            entity.Id.Should().Be(5);
            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public void Entity_Maps_To_StateAutoCompleteDTO()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.States
            {
                Id = 1,
                StateCode = "KA",
                StateName = "Karnataka"
            };

            // Act
            var dto = _mapper.Map<StateAutoCompleteDTO>(entity);

            // Assert
            dto.Id.Should().Be(1);
            dto.StateCode.Should().Be("KA");
            dto.StateName.Should().Be("Karnataka");
        }
    }
}
