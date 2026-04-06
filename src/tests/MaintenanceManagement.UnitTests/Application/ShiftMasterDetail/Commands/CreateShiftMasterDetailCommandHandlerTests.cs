using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.CreateShiftMasterDetail;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ShiftMasterDetail.Commands
{
    public sealed class CreateShiftMasterDetailCommandHandlerTests
    {
        private readonly Mock<IShiftMasterDetailCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateShiftMasterDetailCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateShiftMasterDetailCommand ValidCommand() => new()
        {
            ShiftMasterId = 1,
            UnitId = 1,
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0)
        };

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ShiftMasterDetail>(It.IsAny<CreateShiftMasterDetailCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.ShiftMasterDetail());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.ShiftMasterDetail>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(newId: 4);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(4);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsyncOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.ShiftMasterDetail>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "Shift Master detail"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ReturnsFailureResponse()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ShiftMasterDetail>(It.IsAny<CreateShiftMasterDetailCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.ShiftMasterDetail());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.ShiftMasterDetail>()))
                .ReturnsAsync(0);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_DoesNotPublishAuditEvent()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ShiftMasterDetail>(It.IsAny<CreateShiftMasterDetailCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.ShiftMasterDetail());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.ShiftMasterDetail>()))
                .ReturnsAsync(0);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
