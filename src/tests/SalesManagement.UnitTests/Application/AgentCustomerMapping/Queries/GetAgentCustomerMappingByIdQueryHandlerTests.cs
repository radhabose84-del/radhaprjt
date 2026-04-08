using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingById;

namespace SalesManagement.UnitTests.Application.AgentCustomerMapping.Queries
{
    public class GetAgentCustomerMappingByIdQueryHandlerTests
    {
        private readonly Mock<IAgentCustomerMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAgentCustomerMappingByIdQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAgentCustomerMappingByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsMappedDto()
        {
            var dto = new AgentCustomerMappingDto { Id = 1, CustomerId = 10, AgentId = 20 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<AgentCustomerMappingDto>(dto)).Returns(dto);

            var result = await CreateSut().Handle(
                new GetAgentCustomerMappingByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((AgentCustomerMappingDto?)null);

            var result = await CreateSut().Handle(
                new GetAgentCustomerMappingByIdQuery { Id = 99 },
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_CallsGetByIdAsync_Once()
        {
            var dto = new AgentCustomerMappingDto { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<AgentCustomerMappingDto>(dto)).Returns(dto);

            await CreateSut().Handle(
                new GetAgentCustomerMappingByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}
