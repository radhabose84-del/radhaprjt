using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Application.Complaint.Queries.GetPendingComplaint;

namespace SalesManagement.UnitTests.Application.Complaint.Queries;

public sealed class GetPendingComplaintQueryHandlerTests
{
    private readonly Mock<IComplaintQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetPendingComplaintQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        var dtoList = new List<ComplaintHeaderDto> { new ComplaintHeaderDto { Id = 1 } };
        _mockQueryRepo
            .Setup(r => r.GetPendingAsync(1, 10, null))
            .ReturnsAsync((dtoList, 1));

        var result = await CreateSut().Handle(
            new GetPendingComplaintQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetPendingAsync(1, 10, null))
            .ReturnsAsync((new List<ComplaintHeaderDto>(), 0));

        var result = await CreateSut().Handle(
            new GetPendingComplaintQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
