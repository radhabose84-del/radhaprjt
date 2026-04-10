using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetAllRfq;

namespace PurchaseManagement.UnitTests.Application.Rfqs.Queries
{
    public sealed class GetAllRfqQueryHandlerTests
    {
        private readonly Mock<IRfqQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllRfqQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsItemsAndTotal()
        {
            var items = new List<RfqListItemDto>();
            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<RfqListItemDto>)items, 0));

            _mockMapper
                .Setup(m => m.Map<List<RfqListItemDto>>(It.IsAny<object>()))
                .Returns(items);

            var (result, total) = await CreateSut().Handle(
                new GetAllRfqQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Should().NotBeNull();
            total.Should().Be(0);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<RfqListItemDto>)new List<RfqListItemDto>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<RfqListItemDto>>(It.IsAny<object>()))
                .Returns(new List<RfqListItemDto>());

            await CreateSut().Handle(
                new GetAllRfqQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
