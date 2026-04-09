using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.ComplaintQCReview.Dto;
using SalesManagement.Application.ComplaintQCReview.Queries.GetQCReviewById;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ComplaintQCReview.Queries;

public sealed class GetQCReviewByIdQueryHandlerTests
{
    private readonly Mock<IComplaintQCReviewQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetQCReviewByIdQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingId_ReturnsDto()
    {
        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new ComplaintQCReviewDto { Id = 1 });

        var result = await CreateSut().Handle(
            new GetQCReviewByIdQuery { Id = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsNull()
    {
        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((ComplaintQCReviewDto?)null);

        var result = await CreateSut().Handle(
            new GetQCReviewByIdQuery { Id = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ExistingId_PublishesAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new ComplaintQCReviewDto { Id = 1 });

        await CreateSut().Handle(new GetQCReviewByIdQuery { Id = 1 }, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GetQCReviewByIdQuery"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
