using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Application.Complaint.Queries.GetComplaintsForSalesReturn;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.Complaint.Queries;

public sealed class GetComplaintsForSalesReturnQueryHandlerTests
{
    private readonly Mock<IComplaintQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetComplaintsForSalesReturnQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsComplaints()
    {
        var list = new List<ComplaintForSalesReturnLookupDto>
        {
            new ComplaintForSalesReturnLookupDto { Id = 1, ComplaintNumber = "CMP001", CustomerId = 10 }
        } as IReadOnlyList<ComplaintForSalesReturnLookupDto>;

        _mockQueryRepo
            .Setup(r => r.GetComplaintsForSalesReturnAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await CreateSut().Handle(
            new GetComplaintsForSalesReturnQuery("test"), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].ComplaintNumber.Should().Be("CMP001");
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmpty()
    {
        var emptyList = new List<ComplaintForSalesReturnLookupDto>() as IReadOnlyList<ComplaintForSalesReturnLookupDto>;

        _mockQueryRepo
            .Setup(r => r.GetComplaintsForSalesReturnAsync("", It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        var result = await CreateSut().Handle(
            new GetComplaintsForSalesReturnQuery(""), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NullTerm_PassesEmptyString()
    {
        var list = new List<ComplaintForSalesReturnLookupDto>() as IReadOnlyList<ComplaintForSalesReturnLookupDto>;

        _mockQueryRepo
            .Setup(r => r.GetComplaintsForSalesReturnAsync("", It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await CreateSut().Handle(
            new GetComplaintsForSalesReturnQuery(null!), CancellationToken.None);

        result.Should().BeEmpty();
        _mockQueryRepo.Verify(
            r => r.GetComplaintsForSalesReturnAsync("", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PublishesAuditEvent()
    {
        var list = new List<ComplaintForSalesReturnLookupDto>
        {
            new ComplaintForSalesReturnLookupDto { Id = 1 }
        } as IReadOnlyList<ComplaintForSalesReturnLookupDto>;

        _mockQueryRepo
            .Setup(r => r.GetComplaintsForSalesReturnAsync("", It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        await CreateSut().Handle(
            new GetComplaintsForSalesReturnQuery(""), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionCode == "GetComplaintsForSalesReturnQuery"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
