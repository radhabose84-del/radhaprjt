using AutoMapper;
using MediatR;
using SalesManagement.Application.AgentCommissionConfig.Dto;
using SalesManagement.Application.AgentCommissionConfig.Queries.GetAllAgentCommissionConfig;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.AgentCommissionConfig.Queries
{
    public class GetAllAgentCommissionConfigQueryHandlerTests
    {
        private readonly Mock<IAgentCommissionConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAllAgentCommissionConfigQueryHandler CreateSut()
        {
            _mockMapper
                .Setup(m => m.Map<List<AgentCommissionConfigDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<AgentCommissionConfigDto> ?? new List<AgentCommissionConfigDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return new GetAllAgentCommissionConfigQueryHandler(
                _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsPagedResult_WithCorrectData()
        {
            var list = new List<AgentCommissionConfigDto>
            {
                AgentCommissionConfigBuilders.ValidDto(id: 1),
                AgentCommissionConfigBuilders.ValidDto(id: 2)
            };

            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((list, 2));

            var query = new GetAllAgentCommissionConfigQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsSuccessWithEmptyData()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<AgentCommissionConfigDto>(), 0));

            var query = new GetAllAgentCommissionConfigQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_PassesSearchTermToRepository()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, "Agent001"))
                .ReturnsAsync((new List<AgentCommissionConfigDto>(), 0));

            var query = new GetAllAgentCommissionConfigQuery { PageNumber = 1, PageSize = 10, SearchTerm = "Agent001" };

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 10, "Agent001"), Times.Once);
        }

        [Fact]
        public async Task Handle_PageNumberAndSizeReflectedInResult()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(3, 5, null))
                .ReturnsAsync((new List<AgentCommissionConfigDto>(), 0));

            var query = new GetAllAgentCommissionConfigQuery { PageNumber = 3, PageSize = 5, SearchTerm = null };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.PageNumber.Should().Be(3);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_PublishesAuditLogEvent_Once()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<AgentCommissionConfigDto>(), 0));

            await CreateSut().Handle(
                new GetAllAgentCommissionConfigQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
