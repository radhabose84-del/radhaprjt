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

        [Fact]
        public async Task Handle_ShouldForward_StatusId_And_LastSubmitDate_ToRepository()
        {
            // The handler must pass the StatusId and submission date through to
            // the repository unchanged.
            var date = new DateOnly(2026, 5, 16);

            _mockRepo
                .Setup(r => r.GetRfqAutoCompleteComparisonAsync(
                    "knit", date, 55, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RfqAutoCompleteDto>
                {
                    new() { Id = 1230, RfqCode = "RFQ-Knit-36", LastSubmitDate = date }
                });

            var result = await CreateSut().Handle(
                new GetRfqAutoCompleteComparisonQuery
                {
                    SearchPattern = "knit",
                    LastSubmitDate = date,
                    StatusId = 55
                },
                CancellationToken.None);

            result.Should().ContainSingle(x => x.Id == 1230);
            _mockRepo.Verify(r => r.GetRfqAutoCompleteComparisonAsync(
                "knit", date, 55, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
