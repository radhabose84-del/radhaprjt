using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Dto;
using SalesManagement.Application.SalesEnquiry.Queries.GetAllSalesEnquiry;

namespace SalesManagement.UnitTests.Application.SalesEnquiry.Queries;

public sealed class GetAllSalesEnquiryQueryHandlerTests
{
    private readonly Mock<ISalesEnquiryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetAllSalesEnquiryQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        var dtoList = new List<SalesEnquiryHeaderDto> { new() { Id = 1 } };
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null))
            .ReturnsAsync((dtoList, 1));

        var result = await CreateSut().Handle(
            new GetAllSalesEnquiryQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ReturnsPaginationMetadata()
    {
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(2, 5, "test"))
            .ReturnsAsync((new List<SalesEnquiryHeaderDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllSalesEnquiryQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
            CancellationToken.None);

        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null))
            .ReturnsAsync((new List<SalesEnquiryHeaderDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllSalesEnquiryQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
