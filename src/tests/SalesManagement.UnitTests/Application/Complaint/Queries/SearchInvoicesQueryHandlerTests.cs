using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Application.Complaint.Queries.SearchInvoices;

namespace SalesManagement.UnitTests.Application.Complaint.Queries;

public sealed class SearchInvoicesQueryHandlerTests
{
    private readonly Mock<IComplaintQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private SearchInvoicesQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        var dtoList = new List<InvoiceSearchDto> { new InvoiceSearchDto { InvoiceHeaderId = 1 } };
        _mockQueryRepo
            .Setup(r => r.SearchInvoicesAsync(1, null, false, 1, 10))
            .ReturnsAsync((dtoList, 1));

        var result = await CreateSut().Handle(
            new SearchInvoicesQuery { PartyId = 1, PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.SearchInvoicesAsync(1, null, false, 1, 10))
            .ReturnsAsync((new List<InvoiceSearchDto>(), 0));

        var result = await CreateSut().Handle(
            new SearchInvoicesQuery { PartyId = 1, PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
