using AutoMapper;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Application.State.Commands.UpdateState;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using MediatR;
using FluentValidation;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Application.State.Commands
{
    public class UpdateStateCommandHandlerTests
    {
        private readonly Mock<IStateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IStateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private UpdateStateCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(UpdateStateCommand command)
        {
            var existingState = StateBuilders.ValidEntity(id: command.Id);
            existingState.IsActive = Status.Active;

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existingState);
            _mockQueryRepo
                .Setup(r => r.IsLinkedWithCitiesAsync(command.Id))
                .ReturnsAsync(false);
            _mockCommandRepo
                .Setup(r => r.CountryExistsAsync(command.CountryId))
                .ReturnsAsync(true);
            _mockCommandRepo
                .Setup(r => r.GetStateByCodeAsync(
                    command.StateName ?? string.Empty,
                    command.StateCode ?? string.Empty,
                    command.CountryId))
                .ReturnsAsync(new States { Id = 0 });
            _mockMapper
                .Setup(m => m.Map<States>(command))
                .Returns(StateBuilders.ValidEntity(id: command.Id));
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<States>()))
                .ReturnsAsync(1);

            var updatedState = StateBuilders.ValidEntity(id: command.Id);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(updatedState);

            var dto = StateBuilders.ValidDto(id: command.Id);
            _mockMapper
                .Setup(m => m.Map<StateDto>(It.IsAny<States>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = StateBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var sut = CreateSut();
            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_StateNotFound_ThrowsValidationException()
        {
            var command = StateBuilders.ValidUpdateCommand(id: 999);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((States?)null);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_InvalidCountryId_ThrowsValidationException()
        {
            var command = StateBuilders.ValidUpdateCommand(countryId: 999);
            var existingState = StateBuilders.ValidEntity(id: command.Id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existingState);
            _mockQueryRepo
                .Setup(r => r.IsLinkedWithCitiesAsync(command.Id))
                .ReturnsAsync(false);
            _mockCommandRepo
                .Setup(r => r.CountryExistsAsync(999))
                .ReturnsAsync(false);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Country does not exist*");
        }

        [Fact]
        public async Task Handle_StatusChangeOnly_ReturnsTrue()
        {
            var command = StateBuilders.ValidUpdateCommand(isActive: 1);
            var existingState = StateBuilders.ValidEntity(id: command.Id);
            existingState.IsActive = Status.Inactive; // Different from command's isActive=1

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existingState);
            _mockQueryRepo
                .Setup(r => r.IsLinkedWithCitiesAsync(command.Id))
                .ReturnsAsync(false);
            _mockCommandRepo
                .Setup(r => r.CountryExistsAsync(command.CountryId))
                .ReturnsAsync(true);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<States>()))
                .ReturnsAsync(1);

            var sut = CreateSut();
            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_InactivateLinkedState_ThrowsValidationException()
        {
            var command = StateBuilders.ValidUpdateCommand(isActive: 0);
            var existingState = StateBuilders.ValidEntity(id: command.Id);
            existingState.IsActive = Status.Active;

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existingState);
            _mockQueryRepo
                .Setup(r => r.IsLinkedWithCitiesAsync(command.Id))
                .ReturnsAsync(true);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*linked*");
        }
    }
}
