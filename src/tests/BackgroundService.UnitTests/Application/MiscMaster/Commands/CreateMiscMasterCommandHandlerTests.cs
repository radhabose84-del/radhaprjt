using AutoMapper;
using BackgroundService.Application.Interfaces.IMiscMaster;
using BackgroundService.Application.MiscMaster.Command.CreateMiscMaster;
using BackgroundService.Application.MiscMaster.Queries.GetMiscMaster;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.MiscMaster.Commands
{
    public sealed class CreateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private static CreateMiscMasterCommand ValidCommand() =>
            new() { MiscTypeId = 1, Code = "TEST001", Description = "Test Description" };

        private static BackgroundService.Domain.Entities.Notification.MiscMaster ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                MiscTypeId = 1,
                Code = "TEST001",
                Description = "Test Description"
            };

        private static GetMiscMasterDto ValidDto(int id = 1) =>
            new()
            {
                Id = id,
                MiscTypeId = 1,
                Code = "TEST001",
                Description = "Test Description"
            };

        private void SetupHappyPath(int newId = 1)
        {
            var entity = ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.MiscMaster>(It.IsAny<CreateMiscMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(newId))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscMaster>()))
                .Returns(ValidDto(newId));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsMappedDto()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            var result = await sut.Handle(ValidCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Code.Should().Be("TEST001");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsyncOnce()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscMaster>()),
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
                        e.Module == "MiscMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_FetchesCreatedRecordById()
        {
            SetupHappyPath(5);
            var sut = CreateSut();

            await sut.Handle(ValidCommand(), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(5), Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZeroId_ThrowsExceptionRules()
        {
            var entity = ValidEntity(0);

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.MiscMaster>(It.IsAny<CreateMiscMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(0))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(It.IsAny<BackgroundService.Domain.Entities.Notification.MiscMaster>()))
                .Returns(ValidDto(0));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(ValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
