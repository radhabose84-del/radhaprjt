using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Dto;
using SalesManagement.Application.StoTypeMaster.Queries.GetAllStoTypeMaster;

namespace SalesManagement.UnitTests.Application.StoTypeMaster.Queries;

public class GetAllStoTypeMasterQueryHandlerTests
{
    private readonly Mock<IStoTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetAllStoTypeMasterQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<List<StoTypeMasterDto>>(It.IsAny<object>()))
            .Returns<object>(o => o as List<StoTypeMasterDto> ?? new List<StoTypeMasterDto>());
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetAllStoTypeMasterQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
    {
        var data = new List<StoTypeMasterDto> { new() { Id = 1, StoTypeCode = "STO001" } };
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((data, 1));

        var query = new GetAllStoTypeMasterQuery { PageNumber = 1, PageSize = 10 };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPaginationMetadata()
    {
        var data = new List<StoTypeMasterDto> { new() { Id = 1 } };
        _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "STO")).ReturnsAsync((data, 15));

        var query = new GetAllStoTypeMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "STO" };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
    {
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<StoTypeMasterDto>(), 0));

        var query = new GetAllStoTypeMasterQuery { PageNumber = 1, PageSize = 10 };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
