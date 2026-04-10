using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqAutoComplete;

namespace PurchaseManagement.UnitTests.Application.Quotation.RfqEntry.Queries
{
    public sealed class GetAllRfqAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IRfqQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetRfqAutoCompleteQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetRfqAutoCompleteAsync(
                    It.IsAny<string?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RfqAutoCompleteDto>());
            _mockMapper
                .Setup(m => m.Map<List<RfqAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<RfqAutoCompleteDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRfqAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetRfqAutoCompleteQuery
            {
                SearchPattern = "rfq",
                LastSubmitDate = new DateOnly(2026, 1, 1)
            };
            query.SearchPattern.Should().Be("rfq");
            query.LastSubmitDate.Should().Be(new DateOnly(2026, 1, 1));
        }
    }
}
