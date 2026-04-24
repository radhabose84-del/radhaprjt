using MediatR;
using SalesManagement.Application.Common.Interfaces.ILeadConversionFunnel;
using SalesManagement.Application.LeadConversionFunnel.Dto;
using SalesManagement.Application.LeadConversionFunnel.Queries.GetLeadConversionFunnel;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.LeadConversionFunnel.Queries;

public sealed class GetLeadConversionFunnelQueryHandlerTests
{
    private readonly Mock<ILeadConversionFunnelRepository> _mockRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new();

    private GetLeadConversionFunnelQueryHandler CreateSut()
    {
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetLeadConversionFunnelQueryHandler(_mockRepo.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
    {
        var data = new LeadConversionFunnelDto
        {
            Officers = new List<OfficerFunnelDto>
            {
                new() { MarketingOfficerId = 1, EmployeeNo = "E001", EmployeeName = "Alice" }
            }
        };
        _mockRepo
            .Setup(r => r.GetFunnelAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        var result = await CreateSut().Handle(new GetLeadConversionFunnelQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(data);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccess()
    {
        var data = new LeadConversionFunnelDto { Officers = new List<OfficerFunnelDto>() };
        _mockRepo
            .Setup(r => r.GetFunnelAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        var result = await CreateSut().Handle(new GetLeadConversionFunnelQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Officers.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CallsGetFunnelAsync_Once()
    {
        var data = new LeadConversionFunnelDto { Officers = new List<OfficerFunnelDto>() };
        _mockRepo
            .Setup(r => r.GetFunnelAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        await CreateSut().Handle(new GetLeadConversionFunnelQuery(), CancellationToken.None);

        _mockRepo.Verify(r => r.GetFunnelAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PublishesAuditEvent()
    {
        var data = new LeadConversionFunnelDto
        {
            Officers = new List<OfficerFunnelDto>
            {
                new() { MarketingOfficerId = 1 },
                new() { MarketingOfficerId = 2 }
            }
        };
        _mockRepo
            .Setup(r => r.GetFunnelAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        await CreateSut().Handle(new GetLeadConversionFunnelQuery(), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "GetLeadConversionFunnel" &&
                    e.Module == "LeadConversionFunnel"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
