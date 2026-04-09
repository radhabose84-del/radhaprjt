using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingAutoComplete;

namespace SalesManagement.UnitTests.Application.AgentCustomerMapping.Queries
{
    public class GetAgentCustomerMappingAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAgentCustomerMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAgentCustomerMappingAutoCompleteQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAgentCustomerMappingAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsResults_WhenDataExists()
        {
            var data = new List<AgentCustomerMappingLookupDto>
            {
                new() { Id = 1, CustomerName = "Customer A", AgentName = "Agent A" }
            } as IReadOnlyList<AgentCustomerMappingLookupDto>;

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _mockMapper
                .Setup(m => m.Map<List<AgentCustomerMappingLookupDto>>(data))
                .Returns(data.ToList());

            var result = await CreateSut().Handle(
                new GetAgentCustomerMappingAutoCompleteQuery("test"),
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_UsesEmptyString()
        {
            var data = new List<AgentCustomerMappingLookupDto>() as IReadOnlyList<AgentCustomerMappingLookupDto>;
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("", It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _mockMapper
                .Setup(m => m.Map<List<AgentCustomerMappingLookupDto>>(data))
                .Returns(new List<AgentCustomerMappingLookupDto>());

            var result = await CreateSut().Handle(
                new GetAgentCustomerMappingAutoCompleteQuery(null),
                CancellationToken.None);

            result.Should().BeEmpty();
            _mockQueryRepo.Verify(r => r.AutocompleteAsync("", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var data = new List<AgentCustomerMappingLookupDto>() as IReadOnlyList<AgentCustomerMappingLookupDto>;
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("xyz", It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _mockMapper
                .Setup(m => m.Map<List<AgentCustomerMappingLookupDto>>(data))
                .Returns(new List<AgentCustomerMappingLookupDto>());

            var result = await CreateSut().Handle(
                new GetAgentCustomerMappingAutoCompleteQuery("xyz"),
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
