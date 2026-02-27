using AutoMapper;
using MediatR;
using SalesManagement.Application.AgentCommissionConfig.Dto;
using SalesManagement.Application.AgentCommissionConfig.Queries.GetAgentCommissionConfigAutoComplete;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.AgentCommissionConfig.Queries
{
    public class GetAgentCommissionConfigAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAgentCommissionConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAgentCommissionConfigAutoCompleteQueryHandler CreateSut()
        {
            _mockMapper
                .Setup(m => m.Map<List<AgentCommissionConfigLookupDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<AgentCommissionConfigLookupDto>
                    ?? new List<AgentCommissionConfigLookupDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return new GetAgentCommissionConfigAutoCompleteQueryHandler(
                _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookups = (IReadOnlyList<AgentCommissionConfigLookupDto>)AgentCommissionConfigBuilders.ValidLookupList();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("agent", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);

            var result = await CreateSut().Handle(
                new GetAgentCommissionConfigAutoCompleteQuery("agent"), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsResults()
        {
            var lookups = (IReadOnlyList<AgentCommissionConfigLookupDto>)AgentCommissionConfigBuilders.ValidLookupList();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);

            var result = await CreateSut().Handle(
                new GetAgentCommissionConfigAutoCompleteQuery(null!), CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyStringToRepository()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AgentCommissionConfigLookupDto>());

            await CreateSut().Handle(
                new GetAgentCommissionConfigAutoCompleteQuery(null!), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditLogEvent_Once()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AgentCommissionConfigLookupDto>());

            await CreateSut().Handle(
                new GetAgentCommissionConfigAutoCompleteQuery("test"), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
