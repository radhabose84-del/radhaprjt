using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Application.StoReceipt.Dto;
using SalesManagement.Application.StoReceipt.Queries.GetStoReceiptAutoComplete;

namespace SalesManagement.UnitTests.Application.StoReceipt.Queries
{
    public sealed class GetStoReceiptAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IStoReceiptQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetStoReceiptAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var list = new List<StoReceiptLookupDto> { new() } as IReadOnlyList<StoReceiptLookupDto>;
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("SR", It.IsAny<CancellationToken>())).ReturnsAsync(list);

            var result = await CreateSut().Handle(new GetStoReceiptAutoCompleteQuery("SR"), CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
