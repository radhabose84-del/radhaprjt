using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Commands.UpdateSalesSegment;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesSegment.Commands
{
    public class UpdateSalesSegmentCommandHandlerTests
    {
        private readonly Mock<ISalesSegmentCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ISalesSegmentQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateSalesSegmentCommandHandler CreateSut() =>
            new UpdateSalesSegmentCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupMapper()
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesSegment>(It.IsAny<UpdateSalesSegmentCommand>()))
                .Returns((UpdateSalesSegmentCommand cmd) => new SalesManagement.Domain.Entities.SalesSegment
                {
                    Id = cmd.Id,
                    SegmentName = cmd.SegmentName,
                    CurrencyId = cmd.CurrencyId
                });
        }

        private void SetupUpdateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesSegment>()))
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
            var command = SalesSegmentBuilders.ValidUpdateCommand(id: 1);
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessMessage()
        {
            var command = SalesSegmentBuilders.ValidUpdateCommand(id: 1);
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().Contain("updated");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync_Once()
        {
            var command = SalesSegmentBuilders.ValidUpdateCommand(id: 1);
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesSegment>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = SalesSegmentBuilders.ValidUpdateCommand(id: 1, segmentName: "Updated Seg");
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionCode == "SALES_SEGMENT_UPDATE" &&
                        e.ActionName == "1"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
