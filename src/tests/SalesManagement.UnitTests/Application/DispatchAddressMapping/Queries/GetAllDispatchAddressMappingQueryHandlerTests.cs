using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Dto;
using SalesManagement.Application.DispatchAddressMapping.Queries.GetAllDispatchAddressMapping;

namespace SalesManagement.UnitTests.Application.DispatchAddressMapping.Queries;

public class GetAllDispatchAddressMappingQueryHandlerTests
{
    private readonly Mock<IDispatchAddressMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetAllDispatchAddressMappingQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<List<DispatchAddressMappingDto>>(It.IsAny<object>()))
            .Returns<object>(o => o as List<DispatchAddressMappingDto> ?? new List<DispatchAddressMappingDto>());
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetAllDispatchAddressMappingQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
    {
        var data = new List<DispatchAddressMappingDto> { new() { Id = 1 } };
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, null)).ReturnsAsync((data, 1));

        var query = new GetAllDispatchAddressMappingQuery { PageNumber = 1, PageSize = 10 };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPaginationMetadata()
    {
        var data = new List<DispatchAddressMappingDto> { new() { Id = 1 } };
        _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "test", null)).ReturnsAsync((data, 15));

        var query = new GetAllDispatchAddressMappingQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
    {
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, null)).ReturnsAsync((new List<DispatchAddressMappingDto>(), 0));

        var query = new GetAllDispatchAddressMappingQuery { PageNumber = 1, PageSize = 10 };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
