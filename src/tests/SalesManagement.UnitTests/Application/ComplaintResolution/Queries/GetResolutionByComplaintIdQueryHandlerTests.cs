using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.ComplaintResolution.Dto;
using SalesManagement.Application.ComplaintResolution.Queries.GetResolutionByComplaintId;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ComplaintResolution.Queries;

public sealed class GetResolutionByComplaintIdQueryHandlerTests
{
    private readonly Mock<IComplaintResolutionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetResolutionByComplaintIdQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingComplaint_ReturnsDto()
    {
        _mockQueryRepo
            .Setup(r => r.GetByComplaintHeaderIdAsync(1))
            .ReturnsAsync(new ComplaintResolutionDto { Id = 1, ComplaintHeaderId = 1 });

        var result = await CreateSut().Handle(
            new GetResolutionByComplaintIdQuery { ComplaintHeaderId = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ComplaintHeaderId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NoResolution_ReturnsNull()
    {
        _mockQueryRepo
            .Setup(r => r.GetByComplaintHeaderIdAsync(99))
            .ReturnsAsync((ComplaintResolutionDto?)null);

        var result = await CreateSut().Handle(
            new GetResolutionByComplaintIdQuery { ComplaintHeaderId = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ExistingComplaint_PublishesAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetByComplaintHeaderIdAsync(1))
            .ReturnsAsync(new ComplaintResolutionDto { Id = 1, ComplaintHeaderId = 1 });

        await CreateSut().Handle(
            new GetResolutionByComplaintIdQuery { ComplaintHeaderId = 1 }, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GetResolutionByComplaintIdQuery"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoResolution_DoesNotPublishAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetByComplaintHeaderIdAsync(99))
            .ReturnsAsync((ComplaintResolutionDto?)null);

        await CreateSut().Handle(
            new GetResolutionByComplaintIdQuery { ComplaintHeaderId = 99 }, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
