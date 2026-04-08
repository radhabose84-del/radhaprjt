using Contracts.Common;
using Contracts.Interfaces;
using SalesManagement.Application.AgentPortal.Dto;
using SalesManagement.Application.AgentPortal.Queries.GetAgentDashboard;
using SalesManagement.Application.Common.Interfaces.IAgentPortal;

namespace SalesManagement.UnitTests.Application.AgentPortal.Queries;

public sealed class GetAgentDashboardQueryHandlerTests
{
    private readonly Mock<IAgentPortalQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Strict);

    private GetAgentDashboardQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockIpService.Object);

    [Fact]
    public async Task Handle_WhenAgentIdentified_ReturnsSuccess()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns(10);
        _mockQueryRepo.Setup(r => r.GetAgentCustomerIdsAsync(10)).ReturnsAsync(new List<int> { 1, 2 });
        _mockQueryRepo.Setup(r => r.GetDashboardAsync(10, It.IsAny<List<int>>())).ReturnsAsync(new AgentDashboardDto());

        var result = await CreateSut().Handle(new GetAgentDashboardQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Success");
    }

    [Fact]
    public async Task Handle_WhenAgentIdentified_ReturnsDashboardData()
    {
        var dashboardDto = new AgentDashboardDto();
        _mockIpService.Setup(s => s.GetPartyId()).Returns(10);
        _mockQueryRepo.Setup(r => r.GetAgentCustomerIdsAsync(10)).ReturnsAsync(new List<int> { 1 });
        _mockQueryRepo.Setup(r => r.GetDashboardAsync(10, It.IsAny<List<int>>())).ReturnsAsync(dashboardDto);

        var result = await CreateSut().Handle(new GetAgentDashboardQuery(), CancellationToken.None);

        result.Data.Should().BeSameAs(dashboardDto);
    }

    [Fact]
    public async Task Handle_WhenPartyIdNull_ReturnsFailure()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns((int?)null);

        var result = await CreateSut().Handle(new GetAgentDashboardQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Agent not identified.");
    }

    [Fact]
    public async Task Handle_WhenPartyIdNull_DoesNotCallRepository()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns((int?)null);

        await CreateSut().Handle(new GetAgentDashboardQuery(), CancellationToken.None);

        _mockQueryRepo.Verify(
            r => r.GetAgentCustomerIdsAsync(It.IsAny<int>()), Times.Never);
        _mockQueryRepo.Verify(
            r => r.GetDashboardAsync(It.IsAny<int>(), It.IsAny<List<int>>()), Times.Never);
    }
}
