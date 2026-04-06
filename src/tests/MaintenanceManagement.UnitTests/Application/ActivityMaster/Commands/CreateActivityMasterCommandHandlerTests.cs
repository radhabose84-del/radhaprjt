using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.ActivityMaster.Command.CreateActivityMaster;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ActivityMaster.Commands
{
    public sealed class CreateActivityMasterCommandHandlerTests
    {
        private readonly Mock<IActivityMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IActivityMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateActivityMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static CreateActivityMasterCommand ValidCommand() => new()
        {
            CreateActivityMasterDto = new CreateActivityMasterDto
            {
                ActivityName = "Test Activity",
                UnitId = 1,
                DepartmentId = 1
            }
        };

        private void SetupHappyPath(int newId = 1)
        {
            var entity = new MaintenanceManagement.Domain.Entities.ActivityMaster { Id = newId };
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ActivityMaster>(It.IsAny<CreateActivityMasterDto>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.ActivityMaster>()))
                .ReturnsAsync(entity);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveId()
        {
            SetupHappyPath(newId: 5);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsyncOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.ActivityMaster>()),
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
                        e.Module == "ActivityMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZeroId_ThrowsExceptionRules()
        {
            var entity = new MaintenanceManagement.Domain.Entities.ActivityMaster { Id = 0 };
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ActivityMaster>(It.IsAny<CreateActivityMasterDto>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.ActivityMaster>()))
                .ReturnsAsync(entity);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*ActivityMaster Creation Failed*");
        }
    }
}
