using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.WorkCenter.Command.CreateWorkCenter;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Domain.Events;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;
using Xunit;

namespace MaintenanceManagement.UnitTests.Application.WorkCenter.Commands
{
    public sealed class CreateWorkCenterCommandHandlerTests
    {
        private readonly Mock<IWorkCenterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateWorkCenterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.WorkCenter>(It.IsAny<object>()))
                .Returns(WorkCenterBuilders.ValidEntity(newId));

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.WorkCenter>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(5);
            var result = await CreateSut().Handle(
                WorkCenterBuilders.ValidCreateCommand(), CancellationToken.None);
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(WorkCenterBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.WorkCenter>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(WorkCenterBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ReturnsFailure()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.WorkCenter>(It.IsAny<object>()))
                .Returns(WorkCenterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.WorkCenter>()))
                .ReturnsAsync(0);

            var result = await CreateSut().Handle(
                WorkCenterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
