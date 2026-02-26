using AutoMapper;
using MediatR;
using SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.BusinessUnit.Commands
{
    public class CreateBusinessUnitCommandHandlerTests
    {
        private readonly Mock<IBusinessUnitCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateBusinessUnitCommandHandler CreateSut() =>
            new CreateBusinessUnitCommandHandler(
                _mockCommandRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        private void SetupMapper(CreateBusinessUnitCommand cmd,
            SalesManagement.Domain.Entities.BusinessUnit? entity = null)
        {
            entity ??= new SalesManagement.Domain.Entities.BusinessUnit
            {
                BusinessUnitCode = cmd.BusinessUnitCode,
                BusinessUnitName = cmd.BusinessUnitName,
                Description = cmd.Description
            };
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.BusinessUnit>(cmd))
                .Returns(entity);
        }

        private void SetupCreateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.BusinessUnit>()))
                .ReturnsAsync(returnId);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = BusinessUnitBuilders.ValidCreateCommand();
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
            var command = BusinessUnitBuilders.ValidCreateCommand();
            SetupMapper(command);
            SetupCreateAsync(42);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = BusinessUnitBuilders.ValidCreateCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.BusinessUnit>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = BusinessUnitBuilders.ValidCreateCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "BUSINESSUNIT_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_AuditEvent_ContainsBusinessUnitCode()
        {
            var command = BusinessUnitBuilders.ValidCreateCommand(code: "BU999");
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "BU999"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsEntityActiveAndNotDeleted()
        {
            var command = BusinessUnitBuilders.ValidCreateCommand(code: "BU001");
            SalesManagement.Domain.Entities.BusinessUnit? capturedEntity = null;

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.BusinessUnit>(command))
                .Returns(new SalesManagement.Domain.Entities.BusinessUnit
                {
                    BusinessUnitCode = "BU001",
                    BusinessUnitName = command.BusinessUnitName
                });
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.BusinessUnit>()))
                .Callback<SalesManagement.Domain.Entities.BusinessUnit>(e => capturedEntity = e)
                .ReturnsAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity!.IsActive.Should().Be(SalesManagement.Domain.Common.BaseEntity.Status.Active);
            capturedEntity!.IsDeleted.Should().Be(SalesManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted);
        }
    }
}
