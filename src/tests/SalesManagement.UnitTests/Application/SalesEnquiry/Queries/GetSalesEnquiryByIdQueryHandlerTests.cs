using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Dto;
using SalesManagement.Application.SalesEnquiry.Queries.GetSalesEnquiryById;

namespace SalesManagement.UnitTests.Application.SalesEnquiry.Queries;

public sealed class GetSalesEnquiryByIdQueryHandlerTests
{
    private readonly Mock<ISalesEnquiryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetSalesEnquiryByIdQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingId_ReturnsDto()
    {
        var dto = new SalesEnquiryHeaderDto { Id = 1, PartyId = 1 };
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

        var result = await CreateSut().Handle(
            new GetSalesEnquiryByIdQuery { Id = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsNull()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesEnquiryHeaderDto?)null);

        var result = await CreateSut().Handle(
            new GetSalesEnquiryByIdQuery { Id = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }
}
