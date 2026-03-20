using AutoMapper;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Application.State.Commands.DeleteState;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using MediatR;
using FluentValidation;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Application.State.Commands
{
    public class DeleteStateCommandHandlerTests
    {
        private readonly Mock<IStateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IStateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeleteStateCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsStateDto()
        {
            var command = StateBuilders.ValidDeleteCommand(id: 1);
            var existingState = StateBuilders.ValidEntity(id: 1);
            var mappedEntity = new States { Id = 1 };
            var dto = StateBuilders.ValidDto(id: 1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingState);
            _mockQueryRepo
                .Setup(r => r.GetCityByStateAsync(1))
                .ReturnsAsync(new List<States>());
            _mockMapper
                .Setup(m => m.Map<States>(command))
                .Returns(mappedEntity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<States>()))
                .ReturnsAsync(1);
            _mockMapper
                .Setup(m => m.Map<StateDto>(It.IsAny<States>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_StateNotFound_ThrowsValidationException()
        {
            var command = StateBuilders.ValidDeleteCommand(id: 999);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((States?)null);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*does not exist*");
        }

        [Fact]
        public async Task Handle_StateHasCities_ThrowsValidationException()
        {
            var command = StateBuilders.ValidDeleteCommand(id: 1);
            var existingState = StateBuilders.ValidEntity(id: 1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingState);
            _mockQueryRepo
                .Setup(r => r.GetCityByStateAsync(1))
                .ReturnsAsync(new List<States> { new States { Id = 10 } });

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Cannot delete*");
        }

        [Fact]
        public async Task Handle_ValidDelete_PublishesAuditEvent()
        {
            var command = StateBuilders.ValidDeleteCommand(id: 1);
            var existingState = StateBuilders.ValidEntity(id: 1);
            var mappedEntity = new States { Id = 1 };
            var dto = StateBuilders.ValidDto(id: 1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingState);
            _mockQueryRepo
                .Setup(r => r.GetCityByStateAsync(1))
                .ReturnsAsync(new List<States>());
            _mockMapper
                .Setup(m => m.Map<States>(command))
                .Returns(mappedEntity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<States>()))
                .ReturnsAsync(1);
            _mockMapper
                .Setup(m => m.Map<StateDto>(It.IsAny<States>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "State"),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Delete"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
