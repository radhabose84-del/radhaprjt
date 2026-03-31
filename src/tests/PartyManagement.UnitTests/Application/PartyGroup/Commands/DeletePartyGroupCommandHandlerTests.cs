using AutoMapper;
using Contracts.Common;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Application.PartyGroup.Command.DeletePartyGroup;
using PartyManagement.Domain.Events;
using PartyManagement.UnitTests.TestData;
using Xunit;

namespace PartyManagement.UnitTests.Application.PartyGroup.Commands
{
    public sealed class DeletePartyGroupCommandHandlerTests
    {
        private readonly Mock<IPartyGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeletePartyGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(bool result = true)
        {
            var entity = PartyGroupBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.PartyGroup>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<PartyManagement.Domain.Entities.PartyGroup>()))
                .ReturnsAsync(result);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupHappyPath(true);
            var command = PartyGroupBuilders.ValidDeleteCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            SetupHappyPath();
            var command = PartyGroupBuilders.ValidDeleteCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<PartyManagement.Domain.Entities.PartyGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsExceptionRules()
        {
            var entity = PartyGroupBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.PartyGroup>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<PartyManagement.Domain.Entities.PartyGroup>()))
                .ReturnsAsync(false);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () =>
                await CreateSut().Handle(PartyGroupBuilders.ValidDeleteCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*deletion failed*");
        }
    }
}
