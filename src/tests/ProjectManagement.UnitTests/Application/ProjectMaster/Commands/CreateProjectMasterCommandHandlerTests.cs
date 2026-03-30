using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using ProjectManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using ProjectManagement.Application.Common.Interfaces.IOutbox;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Application.ProjectMaster.Command.CreateProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using ProjectManagement.Domain.Events;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.ProjectMaster.Commands
{
    public sealed class CreateProjectMasterCommandHandlerTests
    {
        private readonly Mock<IProjectMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateProjectMasterCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZone = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);

        private CreateProjectMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockLogger.Object,
                _mockMediator.Object, _mockIp.Object, _mockTimeZone.Object, _mockOutbox.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = ProjectMasterBuilders.ValidEntity(newId);
            var dto = new ProjectMasterDto { Id = newId, ProjectCode = "PRJ001" };

            _mockMapper
                .Setup(m => m.Map<ProjectManagement.Domain.Entities.ProjectMaster>(It.IsAny<object>()))
                .Returns(entity);

            _mockMapper
                .Setup(m => m.Map<ProjectMasterDto>(It.IsAny<object>()))
                .Returns(dto);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<ProjectManagement.Domain.Entities.ProjectMaster>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(newId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockOutbox
                .Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(ProjectMasterBuilders.ValidCreateCommand(), CancellationToken.None);
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(ProjectMasterBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<ProjectManagement.Domain.Entities.ProjectMaster>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(ProjectMasterBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SchedulesOutboxEvent()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(ProjectMasterBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockOutbox.Verify(
                o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
