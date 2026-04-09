using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Dto;
using SalesManagement.Application.SalesReturn.Queries.GetComplaintReturnData;

namespace SalesManagement.UnitTests.Application.SalesReturn.Queries;

public sealed class GetComplaintReturnDataQueryHandlerTests
{
    private readonly Mock<ISalesReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetComplaintReturnDataQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingComplaint_ReturnsSuccess()
    {
        _mockQueryRepo.Setup(r => r.GetComplaintReturnDataAsync(1))
            .ReturnsAsync(new ComplaintReturnDataDto { ComplaintHeaderId = 1 });

        var result = await CreateSut().Handle(
            new GetComplaintReturnDataQuery { ComplaintHeaderId = 1 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ComplaintHeaderId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NonExistentComplaint_ReturnsNotEligible()
    {
        _mockQueryRepo.Setup(r => r.GetComplaintReturnDataAsync(99))
            .ReturnsAsync((ComplaintReturnDataDto?)null);

        var result = await CreateSut().Handle(
            new GetComplaintReturnDataQuery { ComplaintHeaderId = 99 },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("not found or not eligible");
    }
}
