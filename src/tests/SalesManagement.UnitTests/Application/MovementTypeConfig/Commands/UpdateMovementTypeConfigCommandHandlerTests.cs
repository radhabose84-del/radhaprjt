using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Commands.UpdateMovementTypeConfig;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.MovementTypeConfig.Commands
{
    public class UpdateMovementTypeConfigCommandHandlerTests
    {
        private readonly Mock<IMovementTypeConfigCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMovementTypeConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateMovementTypeConfigCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateMovementTypeConfigCommand ValidCommand() => new()
        {
            Id = 1,
            MovementDescription = "Updated Movement",
            MovementCategoryId = 1,
            FromStockTypeId = 2,
            ToStockTypeId = 3,
            IsActive = 1
        };

        private void SetupMapper()
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.MovementTypeConfig>(It.IsAny<UpdateMovementTypeConfigCommand>()))
                .Returns((UpdateMovementTypeConfigCommand cmd) => new SalesManagement.Domain.Entities.MovementTypeConfig
                {
                    Id = cmd.Id,
                    MovementDescription = cmd.MovementDescription,
                    MovementCategoryId = cmd.MovementCategoryId,
                    FromStockTypeId = cmd.FromStockTypeId,
                    ToStockTypeId = cmd.ToStockTypeId
                });
        }

        private void SetupUpdateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.MovementTypeConfig>()))
                .ReturnsAsync(returnId);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccess()
        {
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccessMessage()
        {
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Message.Should().Contain("updated");
        }

        [Fact]
        public async Task Handle_EntityExists_CallsUpdateAsync_Once()
        {
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.MovementTypeConfig>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
        {
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "MOVEMENT_TYPE_CONFIG_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_AuditEvent_ContainsEntityId()
        {
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "1"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
