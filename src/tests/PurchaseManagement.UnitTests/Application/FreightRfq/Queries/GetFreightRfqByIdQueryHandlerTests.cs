using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Dto;
using PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqById;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.FreightRfq.Queries
{
    public sealed class GetFreightRfqByIdQueryHandlerTests
    {
        private readonly Mock<IFreightRfqQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetFreightRfqByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(FreightRfqBuilders.ValidDto(1));

            var result = await CreateSut().Handle(new GetFreightRfqByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_MissingId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((FreightRfqDto?)null);

            var result = await CreateSut().Handle(new GetFreightRfqByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_MissingId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((FreightRfqDto?)null);

            await CreateSut().Handle(new GetFreightRfqByIdQuery { Id = 99 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
