using AutoMapper;
using Contracts.Common;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Application.PartyGroup.Command.UpdatePartyGroup;
using PartyManagement.Domain.Events;
using PartyManagement.UnitTests.TestData;
using Xunit;

namespace PartyManagement.UnitTests.Application.PartyGroup.Commands
{
    public sealed class UpdatePartyGroupCommandHandlerTests
    {
        private readonly Mock<IPartyGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdatePartyGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(bool result = true)
        {
            var entity = PartyGroupBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.PartyGroup>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<PartyManagement.Domain.Entities.PartyGroup>()))
                .ReturnsAsync(result);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath(true);
            var command = PartyGroupBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            var command = PartyGroupBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<PartyManagement.Domain.Entities.PartyGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsExceptionRules()
        {
            var entity = PartyGroupBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.PartyGroup>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<PartyManagement.Domain.Entities.PartyGroup>()))
                .ReturnsAsync(false);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () =>
                await CreateSut().Handle(PartyGroupBuilders.ValidUpdateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Updation Failed*");
        }
    }
}
