using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Queries.GetAgentCommissions;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrder.Queries;

public sealed class GetAgentCommissionsQueryHandlerTests
{
    private readonly Mock<ISalesOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetAgentCommissionsQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsCommissions()
    {
        var commissions = new List<AgentCommissionsDto>
        {
            new AgentCommissionsDto
            {
                Id = 1,
                AgentId = 10,
                AgentName = "Test Agent",
                CommissionPercentage = 5.0m
            }
        };

        _mockQueryRepo
            .Setup(r => r.GetAgentCommissionsAsync(1, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(commissions);

        var query = new GetAgentCommissionsQuery
        {
            SalesGroupId = 1,
            PaymentTermId = 2,
            AgentId = 10
        };

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].AgentName.Should().Be("Test Agent");
        result[0].CommissionPercentage.Should().Be(5.0m);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _mockQueryRepo
            .Setup(r => r.GetAgentCommissionsAsync(99, 99, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentCommissionsDto>());

        var query = new GetAgentCommissionsQuery
        {
            SalesGroupId = 99,
            PaymentTermId = 99,
            AgentId = 99
        };

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CallsRepositoryOnce()
    {
        _mockQueryRepo
            .Setup(r => r.GetAgentCommissionsAsync(1, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentCommissionsDto>());

        var query = new GetAgentCommissionsQuery
        {
            SalesGroupId = 1,
            PaymentTermId = 2,
            AgentId = 10
        };

        await CreateSut().Handle(query, CancellationToken.None);

        _mockQueryRepo.Verify(
            r => r.GetAgentCommissionsAsync(1, 2, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PublishesAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetAgentCommissionsAsync(1, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentCommissionsDto>());

        var query = new GetAgentCommissionsQuery
        {
            SalesGroupId = 1,
            PaymentTermId = 2,
            AgentId = 10
        };

        await CreateSut().Handle(query, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "GetAgentCommissionsQuery" &&
                    e.Module == "SalesOrder"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_AuditEventContainsQueryParameters()
    {
        _mockQueryRepo
            .Setup(r => r.GetAgentCommissionsAsync(5, 3, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentCommissionsDto>());

        var query = new GetAgentCommissionsQuery
        {
            SalesGroupId = 5,
            PaymentTermId = 3,
            AgentId = 7
        };

        await CreateSut().Handle(query, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionName.Contains("SalesGroupId:5") &&
                    e.ActionName.Contains("PaymentTermId:3") &&
                    e.ActionName.Contains("AgentId:7")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
