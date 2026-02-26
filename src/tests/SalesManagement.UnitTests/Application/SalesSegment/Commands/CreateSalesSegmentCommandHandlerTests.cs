using AutoMapper;
using MediatR;
using SalesManagement.Application.SalesSegment.Commands.CreateSalesSegment;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesSegment.Commands
{
    public class CreateSalesSegmentCommandHandlerTests
    {
        private readonly Mock<ISalesSegmentCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateSalesSegmentCommandHandler CreateSut() =>
            new CreateSalesSegmentCommandHandler(
                _mockCommandRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupMapper()
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesSegment>(It.IsAny<CreateSalesSegmentCommand>()))
                .Returns((CreateSalesSegmentCommand cmd) => new SalesManagement.Domain.Entities.SalesSegment
                {
                    SegmentName = cmd.SegmentName,
                    SalesOrganisationId = cmd.SalesOrganisationId,
                    SalesChannelId = cmd.SalesChannelId,
                    BusinessUnitId = cmd.BusinessUnitId,
                    CurrencyId = cmd.CurrencyId
                });
        }

        private void SetupCreateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesSegment>()))
                .ReturnsAsync(returnId);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ── Happy Path ──────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = SalesSegmentBuilders.ValidCreateCommand();
            SetupMapper();
            SetupCreateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = SalesSegmentBuilders.ValidCreateCommand();
            SetupMapper();
            SetupCreateAsync(42);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = SalesSegmentBuilders.ValidCreateCommand();
            SetupMapper();
            SetupCreateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesSegment>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = SalesSegmentBuilders.ValidCreateCommand();
            SetupMapper();
            SetupCreateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALES_SEGMENT_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_AuditEvent_ContainsSegmentName()
        {
            var command = SalesSegmentBuilders.ValidCreateCommand(segmentName: "Finance Segment");
            SetupMapper();
            SetupCreateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "Finance Segment"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
