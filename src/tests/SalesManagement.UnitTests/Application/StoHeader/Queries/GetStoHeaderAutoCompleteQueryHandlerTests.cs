using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Dto;
using SalesManagement.Application.StoHeader.Queries.GetStoHeaderAutoComplete;

namespace SalesManagement.UnitTests.Application.StoHeader.Queries
{
    public sealed class GetStoHeaderAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IStoHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetStoHeaderAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var list = new List<StoHeaderLookupDto> { new() } as IReadOnlyList<StoHeaderLookupDto>;
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("STO", It.IsAny<CancellationToken>())).ReturnsAsync(list);
            _mockMapper.Setup(m => m.Map<List<StoHeaderLookupDto>>(It.IsAny<IReadOnlyList<StoHeaderLookupDto>>()))
                .Returns(new List<StoHeaderLookupDto> { new() });

            var result = await CreateSut().Handle(new GetStoHeaderAutoCompleteQuery("STO"), CancellationToken.None);

            result.Should().NotBeNull();
        }
    }
}
