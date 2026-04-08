using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingByCustomerId;

namespace SalesManagement.UnitTests.Application.AgentCustomerMapping.Queries
{
    public class GetAgentCustomerMappingByCustomerIdQueryHandlerTests
    {
        private readonly Mock<IAgentCustomerMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAgentCustomerMappingByCustomerIdQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAgentCustomerMappingByCustomerIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenDataExists()
        {
            var data = new List<AgentCustomerMappingDto>
            {
                new() { Id = 1, CustomerId = 5, AgentId = 10 },
                new() { Id = 2, CustomerId = 5, AgentId = 11 }
            };
            _mockQueryRepo.Setup(r => r.GetByCustomerIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _mockMapper
                .Setup(m => m.Map<List<AgentCustomerMappingDto>>(data))
                .Returns(data);

            var result = await CreateSut().Handle(
                new GetAgentCustomerMappingByCustomerIdQuery { CustomerId = 5 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithZeroCount()
        {
            var data = new List<AgentCustomerMappingDto>();
            _mockQueryRepo.Setup(r => r.GetByCustomerIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _mockMapper
                .Setup(m => m.Map<List<AgentCustomerMappingDto>>(data))
                .Returns(data);

            var result = await CreateSut().Handle(
                new GetAgentCustomerMappingByCustomerIdQuery { CustomerId = 99 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetByCustomerIdAsync_Once()
        {
            var data = new List<AgentCustomerMappingDto>();
            _mockQueryRepo.Setup(r => r.GetByCustomerIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _mockMapper
                .Setup(m => m.Map<List<AgentCustomerMappingDto>>(data))
                .Returns(data);

            await CreateSut().Handle(
                new GetAgentCustomerMappingByCustomerIdQuery { CustomerId = 5 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByCustomerIdAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
