using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Commands.CreateMovementTypeConfig;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.MovementTypeConfig.Commands
{
    public class CreateMovementTypeConfigCommandHandlerTests
    {
        private readonly Mock<IMovementTypeConfigCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMovementTypeConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateMovementTypeConfigCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateMovementTypeConfigCommand ValidCommand() => new()
        {
            MovementCode = "MOVE01",
            MovementDescription = "Test Movement",
            MovementCategoryId = 1,
            FromStockTypeId = 2,
            ToStockTypeId = 3,
            QuantityUpdateFlag = true
        };

        private void SetupMapper(CreateMovementTypeConfigCommand cmd)
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.MovementTypeConfig>(cmd))
                .Returns(new SalesManagement.Domain.Entities.MovementTypeConfig
                {
                    MovementCode = cmd.MovementCode,
                    MovementDescription = cmd.MovementDescription,
                    MovementCategoryId = cmd.MovementCategoryId,
                    FromStockTypeId = cmd.FromStockTypeId,
                    ToStockTypeId = cmd.ToStockTypeId
                });
        }

        private void SetupCreateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.MovementTypeConfig>()))
                .ReturnsAsync(returnId);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewEntityId()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(42);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.MovementTypeConfig>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "MOVEMENT_TYPE_CONFIG_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_AuditEvent_ContainsMovementCode()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "MOVE01"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
