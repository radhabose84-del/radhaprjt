using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanAutoComplete;

namespace SalesManagement.UnitTests.Application.DeliveryChallan.Queries
{
    public sealed class GetDeliveryChallanAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IDeliveryChallanQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDeliveryChallanAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var list = new List<DeliveryChallanLookupDto> { new DeliveryChallanLookupDto() } as IReadOnlyList<DeliveryChallanLookupDto>;
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("DC", It.IsAny<CancellationToken>())).ReturnsAsync(list);

            var result = await CreateSut().Handle(new GetDeliveryChallanAutoCompleteQuery("DC"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsResults()
        {
            var list = new List<DeliveryChallanLookupDto>() as IReadOnlyList<DeliveryChallanLookupDto>;
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("", It.IsAny<CancellationToken>())).ReturnsAsync(list);

            var result = await CreateSut().Handle(new GetDeliveryChallanAutoCompleteQuery(""), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
