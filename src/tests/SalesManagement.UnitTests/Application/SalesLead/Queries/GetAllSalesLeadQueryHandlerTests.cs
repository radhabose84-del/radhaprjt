using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Dto;
using SalesManagement.Application.SalesLead.Queries.GetAllSalesLead;

namespace SalesManagement.UnitTests.Application.SalesLead.Queries;

public class GetAllSalesLeadQueryHandlerTests
{
    private readonly Mock<ISalesLeadQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetAllSalesLeadQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<List<SalesLeadDto>>(It.IsAny<object>()))
            .Returns<object>(o => o as List<SalesLeadDto> ?? new List<SalesLeadDto>());
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetAllSalesLeadQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
    {
        var data = new List<SalesLeadDto> { new() { Id = 1, ContactName = "Jane" } };
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((data, 1));

        var query = new GetAllSalesLeadQuery { PageNumber = 1, PageSize = 10 };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPaginationMetadata()
    {
        var data = new List<SalesLeadDto> { new() { Id = 1 } };
        _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "search")).ReturnsAsync((data, 15));

        var query = new GetAllSalesLeadQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
    {
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<SalesLeadDto>(), 0));

        var query = new GetAllSalesLeadQuery { PageNumber = 1, PageSize = 10 };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
