using AutoMapper;
using BackgroundService.Application.Common.Interfaces.IMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using BackgroundService.Domain.Events;
using Contracts.Common;
using MediatR;

namespace BackgroundService.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class CreateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private static CreateMiscTypeMasterCommand ValidCommand() =>
            new() { MiscTypeCode = "TESTTYPE", Description = "Test Type Description" };

        private static BackgroundService.Domain.Entities.Notification.MiscTypeMaster ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                MiscTypeCode = "TESTTYPE",
                Description = "Test Type Description"
            };

        private static GetMiscTypeMasterDto ValidDto(int id = 1) =>
            new()
            {
                Id = id,
                MiscTypeCode = "TESTTYPE",
                Description = "Test Type Description"
            };

        private void SetupHappyPath(int newId = 1)
        {
            var entity = ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>(It.IsAny<CreateMiscTypeMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(newId))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>()))
                .Returns(ValidDto(newId));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            var result = await sut.Handle(ValidCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsyncOnce()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "MiscTypeMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCorrectMessage()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            var result = await sut.Handle(ValidCommand(), CancellationToken.None);

            result.Message.Should().Be("Misc Type Master created successfully");
        }

        [Fact]
        public async Task Handle_CreateReturnsZeroId_ReturnsFailure()
        {
            var entity = ValidEntity(0);

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>(It.IsAny<CreateMiscTypeMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>()))
                .ReturnsAsync(entity);

            var sut = CreateSut();

            var result = await sut.Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Failed");
        }

        [Fact]
        public async Task Handle_CreateReturnsZeroId_DoesNotPublishAuditEvent()
        {
            var entity = ValidEntity(0);

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>(It.IsAny<CreateMiscTypeMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>()))
                .ReturnsAsync(entity);

            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
