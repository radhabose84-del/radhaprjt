using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceAutoComplete;

namespace SalesManagement.UnitTests.Application.DispatchAdvice.Queries
{
    public sealed class GetDispatchAdviceAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IDispatchAdviceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDispatchAdviceAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var list = new List<DispatchAdviceLookupDto> { new() } as IReadOnlyList<DispatchAdviceLookupDto>;
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("DA", It.IsAny<CancellationToken>())).ReturnsAsync(list);

            var result = await CreateSut().Handle(new GetDispatchAdviceAutoCompleteQuery("DA"), CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
