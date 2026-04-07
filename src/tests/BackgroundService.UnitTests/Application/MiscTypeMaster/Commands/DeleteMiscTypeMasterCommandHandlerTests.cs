using AutoMapper;
using BackgroundService.Application.Common.Interfaces.IMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using BackgroundService.Domain.Events;
using Contracts.Common;
using MediatR;

namespace BackgroundService.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class DeleteMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static DeleteMiscTypeMasterCommand ValidCommand(int id = 1) =>
            new() { Id = id };

        private void SetupHappyPath(bool deleteResult = true)
        {
            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>(It.IsAny<DeleteMiscTypeMasterCommand>()))
                .Returns(new BackgroundService.Domain.Entities.Notification.MiscTypeMaster { Id = 1 });

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>()))
                .ReturnsAsync(deleteResult);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(true);
            var sut = CreateSut();

            var result = await sut.Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("deleted successfully");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteAsyncOnce()
        {
            SetupHappyPath(true);
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(true);
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "MiscTypeMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteFails_ReturnsFailure()
        {
            SetupHappyPath(false);
            var sut = CreateSut();

            var result = await sut.Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("not deleted");
        }

        [Fact]
        public async Task Handle_DeleteFails_StillPublishesAuditEvent()
        {
            // The handler publishes audit event before checking result
            SetupHappyPath(false);
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
