using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqAutoComplete;

namespace PurchaseManagement.UnitTests.Application.Quotation.RfqEntry.Queries
{
    public sealed class GetRfqAutoCompleteQuotationHandlerTests
    {
        private readonly Mock<IRfqQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private GetRfqAutoCompleteQuotationHandler CreateSut() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetRfqAutoCompleteQuotationAsync(
                    It.IsAny<string?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RfqAutoCompleteDto>());

            var result = await CreateSut().Handle(
                new GetRfqAutoCompleteQuotationQuery { SearchPattern = "test" }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetRfqAutoCompleteQuotationQuery
            {
                SearchPattern = "rfq",
                LastSubmitDate = new DateOnly(2026, 3, 15)
            };
            query.SearchPattern.Should().Be("rfq");
            query.LastSubmitDate.Should().Be(new DateOnly(2026, 3, 15));
        }
    }
}
