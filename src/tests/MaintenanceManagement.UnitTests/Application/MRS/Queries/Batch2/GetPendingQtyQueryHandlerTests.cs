using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMRS;
using MaintenanceManagement.Application.MRS.Queries.GetPendingQty;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MRS.Queries.Batch2
{
    public sealed class GetPendingQtyQueryHandlerTests
    {
        private readonly Mock<IMRSQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPendingQtyQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResult_ReturnsDto()
        {
            var dto = new GetPendingQtyDto();
            _mockQueryRepo
                .Setup(r => r.GetPendingIssueAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<GetPendingQtyDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetPendingQtyQuery { OldUnitcode = "U01", ItemCode = "I01" },
                CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetPendingIssueAsync("U01", "I01"))
                .ReturnsAsync(new GetPendingQtyDto());
            _mockMapper
                .Setup(m => m.Map<GetPendingQtyDto>(It.IsAny<object>()))
                .Returns(new GetPendingQtyDto());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetPendingQtyQuery { OldUnitcode = "U01", ItemCode = "I01" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetPendingIssueAsync("U01", "I01"), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetPendingIssueAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new GetPendingQtyDto());
            _mockMapper
                .Setup(m => m.Map<GetPendingQtyDto>(It.IsAny<object>()))
                .Returns(new GetPendingQtyDto());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetPendingQtyQuery { OldUnitcode = "U01", ItemCode = "I01" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "IssueRequestPending"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
