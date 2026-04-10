using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Application.Complaint.Queries.GetComplaintById;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.Complaint.Queries;

public sealed class GetComplaintByIdQueryHandlerTests
{
    private readonly Mock<IComplaintQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetComplaintByIdQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingId_ReturnsDto()
    {
        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new ComplaintHeaderDto { Id = 1, ComplaintNumber = "CMP001" });

        var result = await CreateSut().Handle(
            new GetComplaintByIdQuery { Id = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsNull()
    {
        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((ComplaintHeaderDto?)null);

        var result = await CreateSut().Handle(
            new GetComplaintByIdQuery { Id = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ExistingId_PublishesAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new ComplaintHeaderDto { Id = 1 });

        await CreateSut().Handle(new GetComplaintByIdQuery { Id = 1 }, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GetComplaintByIdQuery"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
