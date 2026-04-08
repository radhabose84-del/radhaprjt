using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqAutoCompleteComparison;

namespace PurchaseManagement.UnitTests.Application.Quotation.RfqEntry.Queries
{
    public sealed class GetRfqAutoCompleteComparisonHandlerTests
    {
        private readonly Mock<IRfqQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private GetRfqAutoCompleteComparisonHandler CreateSut() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetRfqAutoCompleteComparisonAsync(
                    It.IsAny<string?>(), It.IsAny<DateOnly?>(), It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RfqAutoCompleteDto>());

            var result = await CreateSut().Handle(
                new GetRfqAutoCompleteComparisonQuery { SearchPattern = "test" }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetRfqAutoCompleteComparisonQuery
            {
                SearchPattern = "rfq",
                LastSubmitDate = new DateOnly(2026, 1, 1),
                StatusId = 3
            };
            query.SearchPattern.Should().Be("rfq");
            query.StatusId.Should().Be(3);
        }
    }
}
