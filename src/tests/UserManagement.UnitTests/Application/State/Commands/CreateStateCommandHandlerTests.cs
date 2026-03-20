using AutoMapper;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Application.State.Commands.CreateState;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using MediatR;
using FluentValidation;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Application.State.Commands
{
    public class CreateStateCommandHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IStateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private CreateStateCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsStateDto()
        {
            // Arrange
            var command = StateBuilders.ValidCreateCommand();
            var stateEntity = StateBuilders.ValidEntity();
            var createdEntity = StateBuilders.ValidEntity(id: 1);
            var dto = StateBuilders.ValidDto(id: 1);

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
                .Returns(stateEntity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<States>()))
                .ReturnsAsync(createdEntity);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockMapper
                .Setup(m => m.Map<StateDto>(createdEntity))
                .Returns(dto);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_InvalidCountryId_ThrowsValidationException()
        {
            var command = StateBuilders.ValidCreateCommand(countryId: 999);

            _mockCommandRepo
                .Setup(r => r.CountryExistsAsync(999))
                .ReturnsAsync(false);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*country does not exist*");
        }

        [Fact]
        public async Task Handle_DuplicateState_ThrowsValidationException()
        {
            var command = StateBuilders.ValidCreateCommand();

            _mockCommandRepo
                .Setup(r => r.CountryExistsAsync(command.CountryId))
                .ReturnsAsync(true);
            _mockCommandRepo
                .Setup(r => r.GetStateByCodeAsync(
                    command.StateName ?? string.Empty,
                    command.StateCode ?? string.Empty,
                    command.CountryId))
                .ReturnsAsync(new States { Id = 5 }); // Already exists

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = StateBuilders.ValidCreateCommand();
            var stateEntity = StateBuilders.ValidEntity();
            var createdEntity = StateBuilders.ValidEntity(id: 1);
            var dto = StateBuilders.ValidDto(id: 1);

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
                .Returns(stateEntity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<States>()))
                .ReturnsAsync(createdEntity);
            _mockMediator
                .Setup(m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "State"),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockMapper
                .Setup(m => m.Map<StateDto>(createdEntity))
                .Returns(dto);

            var sut = CreateSut();
            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create" && e.Module == "State"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsMapper()
        {
            var command = StateBuilders.ValidCreateCommand();
            var stateEntity = StateBuilders.ValidEntity();
            var createdEntity = StateBuilders.ValidEntity(id: 1);
            var dto = StateBuilders.ValidDto(id: 1);

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
                .Returns(stateEntity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<States>()))
                .ReturnsAsync(createdEntity);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockMapper
                .Setup(m => m.Map<StateDto>(createdEntity))
                .Returns(dto);

            var sut = CreateSut();
            await sut.Handle(command, CancellationToken.None);

            _mockMapper.Verify(m => m.Map<States>(command), Times.Once);
            _mockMapper.Verify(m => m.Map<StateDto>(createdEntity), Times.Once);
        }
    }
}
