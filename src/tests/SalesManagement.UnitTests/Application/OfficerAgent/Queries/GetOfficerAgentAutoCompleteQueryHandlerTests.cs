using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Dto;
using SalesManagement.Application.OfficerAgent.Queries.GetOfficerAgentAutoComplete;

namespace SalesManagement.UnitTests.Application.OfficerAgent.Queries
{
    public class GetOfficerAgentAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IOfficerAgentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetOfficerAgentAutoCompleteQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<OfficerAgentGroupedDto>>(It.IsAny<object>()))
                .Returns<object>(o => o is IEnumerable<OfficerAgentGroupedDto> e ? e.ToList() : new List<OfficerAgentGroupedDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetOfficerAgentAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_WithTerm_ReturnsResults()
        {
            var lookupList = new List<OfficerAgentGroupedDto>
            {
                new() { MarketingOfficerId = 1, OfficerName = "Officer A" },
                new() { MarketingOfficerId = 2, OfficerName = "Officer B" }
            } as IReadOnlyList<OfficerAgentGroupedDto>;

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("officer", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetOfficerAgentAutoCompleteQuery("officer"), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_WithTerm_CallsAutocompleteAsync_Once()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OfficerAgentGroupedDto>() as IReadOnlyList<OfficerAgentGroupedDto>);

            await CreateSut().Handle(
                new GetOfficerAgentAutoCompleteQuery("test"), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyStringToRepository()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OfficerAgentGroupedDto>() as IReadOnlyList<OfficerAgentGroupedDto>);

            await CreateSut().Handle(
                new GetOfficerAgentAutoCompleteQuery(null), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
