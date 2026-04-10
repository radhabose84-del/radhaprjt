using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonById;
using PurchaseManagement.Application.PartyMaster.Queries.GetPartyMasterById;

namespace PurchaseManagement.UnitTests.Application.QuotationCompare.Queries
{
    public sealed class GetQuoteComparisonByIdQueryHandlerTests
    {
        private readonly Mock<IQuotationCompareQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetQuoteComparisonByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var dto = new QuoteCompareByIdDto { Id = 1 };
            _mockRepo
                .Setup(r => r.GetByIdQuoteCompareAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetQuoteComparisonByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsKeyNotFoundException()
        {
            _mockRepo
                .Setup(r => r.GetByIdQuoteCompareAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((QuoteCompareByIdDto?)null!);

            Func<Task> act = async () =>
                await CreateSut().Handle(new GetQuoteComparisonByIdQuery { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var dto = new QuoteCompareByIdDto { Id = 1 };
            _mockRepo
                .Setup(r => r.GetByIdQuoteCompareAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            await CreateSut().Handle(
                new GetQuoteComparisonByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
