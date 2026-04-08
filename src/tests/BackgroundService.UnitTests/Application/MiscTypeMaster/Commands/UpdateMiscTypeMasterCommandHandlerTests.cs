using AutoMapper;
using BackgroundService.Application.Common.Interfaces.IMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using BackgroundService.Domain.Events;
using Contracts.Common;
using MediatR;

namespace BackgroundService.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class UpdateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static UpdateMiscTypeMasterCommand ValidCommand(int id = 1) =>
            new()
            {
                Id = id,
                MiscTypeCode = "TESTTYPE",
                Description = "Updated Description",
                IsActive = 1
            };

        private void SetupHappyPath(bool updateResult = true)
        {
            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync((BackgroundService.Domain.Entities.Notification.MiscTypeMaster?)null);

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>(It.IsAny<UpdateMiscTypeMasterCommand>()))
                .Returns(new BackgroundService.Domain.Entities.Notification.MiscTypeMaster
                {
                    Id = 1,
                    MiscTypeCode = "TESTTYPE",
                    Description = "Updated Description"
                });

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>()))
                .ReturnsAsync(updateResult);

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
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsyncOnce()
        {
            SetupHappyPath(true);
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>()),
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
                        e.ActionDetail == "Update" &&
                        e.Module == "MiscTypeMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateFails_ReturnsFailure()
        {
            SetupHappyPath(false);
            var sut = CreateSut();

            var result = await sut.Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("not updated");
        }

        [Fact]
        public async Task Handle_DuplicateCode_ReturnsAlreadyExists()
        {
            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync(new BackgroundService.Domain.Entities.Notification.MiscTypeMaster { Id = 99, MiscTypeCode = "TESTTYPE" });

            var sut = CreateSut();

            var result = await sut.Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task Handle_DuplicateCode_DoesNotCallUpdateAsync()
        {
            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync(new BackgroundService.Domain.Entities.Notification.MiscTypeMaster { Id = 99, MiscTypeCode = "TESTTYPE" });

            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>()),
                Times.Never);
        }
    }
}
