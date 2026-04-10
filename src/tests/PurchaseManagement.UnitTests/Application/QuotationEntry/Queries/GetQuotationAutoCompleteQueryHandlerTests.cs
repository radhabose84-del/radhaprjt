using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetQuotationAutoComplete;

namespace PurchaseManagement.UnitTests.Application.QuotationEntry.Queries
{
    public sealed class GetQuotationAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IQuotationQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetQuotationAutoCompleteQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMappedList()
        {
            _mockRepo
                .Setup(r => r.GetQuotationAutoComplete(It.IsAny<string?>()))
                .ReturnsAsync(new List<QuotationAutoCompleteDto>());

            _mockMapper
                .Setup(m => m.Map<List<QuotationAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<QuotationAutoCompleteDto>());

            var result = await CreateSut().Handle(
                new GetQuotationAutoCompleteQuery { SearchPattern = "test" },
                CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockRepo
                .Setup(r => r.GetQuotationAutoComplete(It.IsAny<string?>()))
                .ReturnsAsync(new List<QuotationAutoCompleteDto>());

            _mockMapper
                .Setup(m => m.Map<List<QuotationAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<QuotationAutoCompleteDto>());

            await CreateSut().Handle(
                new GetQuotationAutoCompleteQuery { SearchPattern = "test" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
