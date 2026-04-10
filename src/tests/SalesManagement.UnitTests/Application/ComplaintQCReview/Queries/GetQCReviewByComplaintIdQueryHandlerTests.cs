using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.ComplaintQCReview.Dto;
using SalesManagement.Application.ComplaintQCReview.Queries.GetQCReviewByComplaintId;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ComplaintQCReview.Queries;

public sealed class GetQCReviewByComplaintIdQueryHandlerTests
{
    private readonly Mock<IComplaintQCReviewQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetQCReviewByComplaintIdQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingComplaint_ReturnsDto()
    {
        _mockQueryRepo
            .Setup(r => r.GetByComplaintIdAsync(1))
            .ReturnsAsync(new ComplaintQCReviewDto { Id = 1, ComplaintHeaderId = 1 });

        var result = await CreateSut().Handle(
            new GetQCReviewByComplaintIdQuery { ComplaintHeaderId = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ComplaintHeaderId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NoReview_ReturnsNull()
    {
        _mockQueryRepo
            .Setup(r => r.GetByComplaintIdAsync(99))
            .ReturnsAsync((ComplaintQCReviewDto?)null);

        var result = await CreateSut().Handle(
            new GetQCReviewByComplaintIdQuery { ComplaintHeaderId = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ExistingComplaint_PublishesAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetByComplaintIdAsync(1))
            .ReturnsAsync(new ComplaintQCReviewDto { Id = 1, ComplaintHeaderId = 1 });

        await CreateSut().Handle(
            new GetQCReviewByComplaintIdQuery { ComplaintHeaderId = 1 }, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GetQCReviewByComplaintIdQuery"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoReview_DoesNotPublishAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetByComplaintIdAsync(99))
            .ReturnsAsync((ComplaintQCReviewDto?)null);

        await CreateSut().Handle(
            new GetQCReviewByComplaintIdQuery { ComplaintHeaderId = 99 }, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
