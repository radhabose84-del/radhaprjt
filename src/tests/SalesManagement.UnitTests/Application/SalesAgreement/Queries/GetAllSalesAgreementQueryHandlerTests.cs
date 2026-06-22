using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Application.SalesAgreement.Dto;
using SalesManagement.Application.SalesAgreement.Queries.GetAllSalesAgreement;

namespace SalesManagement.UnitTests.Application.SalesAgreement.Queries;

public class GetAllSalesAgreementQueryHandlerTests
{
    private readonly Mock<ISalesAgreementQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new();

    private GetAllSalesAgreementQueryHandler CreateSut()
    {
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetAllSalesAgreementQueryHandler(_mockQueryRepo.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
    {
        var data = new List<SalesAgreementHeaderDto> { new() { Id = 1, AgreementNo = "SA001" } };
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, null)).ReturnsAsync((data, 1));

        var result = await CreateSut().Handle(new GetAllSalesAgreementQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ReturnsPaginationMetadata()
    {
        var data = new List<SalesAgreementHeaderDto> { new() { Id = 1 } };
        _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "SA", "Pending")).ReturnsAsync((data, 15));

        var result = await CreateSut().Handle(
            new GetAllSalesAgreementQuery { PageNumber = 2, PageSize = 5, SearchTerm = "SA", StatusName = "Pending" },
            CancellationToken.None);

        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, null)).ReturnsAsync((new List<SalesAgreementHeaderDto>(), 0));

        var result = await CreateSut().Handle(new GetAllSalesAgreementQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
