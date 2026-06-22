using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Dto;
using SalesManagement.Application.SalesOrderTypeMaster.Queries.GetAllSalesOrderTypeMaster;

namespace SalesManagement.UnitTests.Application.SalesOrderTypeMaster.Queries;

public class GetAllSalesOrderTypeMasterQueryHandlerTests
{
    private readonly Mock<ISalesOrderTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetAllSalesOrderTypeMasterQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<List<SalesOrderTypeMasterDto>>(It.IsAny<object>()))
            .Returns<object>(o => o as List<SalesOrderTypeMasterDto> ?? new List<SalesOrderTypeMasterDto>());
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetAllSalesOrderTypeMasterQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
    {
        var data = new List<SalesOrderTypeMasterDto> { new() { Id = 1, TypeName = "Normal" } };
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((data, 1));

        var query = new GetAllSalesOrderTypeMasterQuery { PageNumber = 1, PageSize = 10 };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPaginationMetadata()
    {
        var data = new List<SalesOrderTypeMasterDto> { new() { Id = 1 } };
        _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "SO")).ReturnsAsync((data, 15));

        var query = new GetAllSalesOrderTypeMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "SO" };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
    {
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<SalesOrderTypeMasterDto>(), 0));

        var query = new GetAllSalesOrderTypeMasterQuery { PageNumber = 1, PageSize = 10 };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
