using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Dto;
using SalesManagement.Application.SalesReturn.Queries.GetSalesReturnByComplaint;

namespace SalesManagement.UnitTests.Application.SalesReturn.Queries;

public sealed class GetSalesReturnByComplaintQueryHandlerTests
{
    private readonly Mock<ISalesReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetSalesReturnByComplaintQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingComplaint_ReturnsSuccess()
    {
        _mockQueryRepo.Setup(r => r.GetByComplaintIdAsync(1))
            .ReturnsAsync(new SalesReturnHeaderDto { Id = 1, ComplaintHeaderId = 1 });

        var result = await CreateSut().Handle(
            new GetSalesReturnByComplaintQuery { ComplaintHeaderId = 1 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NonExistentComplaint_ReturnsNotFound()
    {
        _mockQueryRepo.Setup(r => r.GetByComplaintIdAsync(99))
            .ReturnsAsync((SalesReturnHeaderDto?)null);

        var result = await CreateSut().Handle(
            new GetSalesReturnByComplaintQuery { ComplaintHeaderId = 99 },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("No Sales Return found");
    }
}
