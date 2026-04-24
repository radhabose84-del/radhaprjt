using AutoMapper;
using MediatR;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingByFilter;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;

namespace SalesManagement.UnitTests.Application.AgentCustomerMapping.Queries;

public sealed class GetAgentCustomerMappingByFilterQueryHandlerTests
{
    private readonly Mock<IAgentCustomerMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetAgentCustomerMappingByFilterQueryHandler CreateSut()
    {
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetAgentCustomerMappingByFilterQueryHandler(
            _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WithFilteredData()
    {
        var data = new List<AgentCustomerMappingDto>
        {
            new() { Id = 1, CustomerId = 5, AgentId = 10 },
            new() { Id = 2, CustomerId = 5, AgentId = 11 }
        };
        _mockQueryRepo
            .Setup(r => r.GetByFilterAsync(3, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((data, 2));
        _mockMapper
            .Setup(m => m.Map<List<AgentCustomerMappingDto>>(data))
            .Returns(data);

        var result = await CreateSut().Handle(
            new GetAgentCustomerMappingByFilterQuery { SalesGroupId = 3, CustomerId = 5 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccessWithZeroCount()
    {
        var data = new List<AgentCustomerMappingDto>();
        _mockQueryRepo
            .Setup(r => r.GetByFilterAsync(99, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((data, 0));
        _mockMapper
            .Setup(m => m.Map<List<AgentCustomerMappingDto>>(data))
            .Returns(data);

        var result = await CreateSut().Handle(
            new GetAgentCustomerMappingByFilterQuery { SalesGroupId = 99, CustomerId = 99 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_NullFilters_PassesThroughToRepository()
    {
        var data = new List<AgentCustomerMappingDto>();
        _mockQueryRepo
            .Setup(r => r.GetByFilterAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((data, 0));
        _mockMapper
            .Setup(m => m.Map<List<AgentCustomerMappingDto>>(data))
            .Returns(data);

        await CreateSut().Handle(
            new GetAgentCustomerMappingByFilterQuery { SalesGroupId = null, CustomerId = null },
            CancellationToken.None);

        _mockQueryRepo.Verify(
            r => r.GetByFilterAsync(null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
