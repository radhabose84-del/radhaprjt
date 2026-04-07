using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAllAgentCustomerMapping;

namespace SalesManagement.UnitTests.Application.AgentCustomerMapping.Queries
{
    public class GetAllAgentCustomerMappingQueryHandlerTests
    {
        private readonly Mock<IAgentCustomerMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAllAgentCustomerMappingQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAllAgentCustomerMappingQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
        {
            var data = new List<AgentCustomerMappingDto>
            {
                new() { Id = 1, CustomerId = 10, AgentId = 20 },
                new() { Id = 2, CustomerId = 11, AgentId = 21 }
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((data, 2));

            var result = await CreateSut().Handle(
                new GetAllAgentCustomerMappingQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            var data = new List<AgentCustomerMappingDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "test"))
                .ReturnsAsync((data, 11));

            var result = await CreateSut().Handle(
                new GetAllAgentCustomerMappingQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.TotalCount.Should().Be(11);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<AgentCustomerMappingDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllAgentCustomerMappingQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsync_Once_WithCorrectParameters()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 20, "search"))
                .ReturnsAsync((new List<AgentCustomerMappingDto>(), 0));

            await CreateSut().Handle(
                new GetAllAgentCustomerMappingQuery { PageNumber = 1, PageSize = 20, SearchTerm = "search" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 20, "search"), Times.Once);
        }
    }
}
