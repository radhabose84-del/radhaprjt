using MediatR;
using PurchaseManagement.Application.Arrival.Queries.GetArrivalAutoComplete;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.Arrival.Queries
{
    public sealed class GetArrivalAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IArrivalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetArrivalAutoCompleteQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ArrivalBuilders.ValidLookupList());

            var result = await CreateSut().Handle(new GetArrivalAutoCompleteQuery("ARV"), CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].ArrivalNumber.Should().Be("ARV-2025-0006");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ArrivalBuilders.ValidLookupList());

            await CreateSut().Handle(new GetArrivalAutoCompleteQuery("ARV"), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
