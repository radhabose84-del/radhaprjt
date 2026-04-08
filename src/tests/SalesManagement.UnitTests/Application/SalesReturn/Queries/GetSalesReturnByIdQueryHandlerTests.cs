using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Dto;
using SalesManagement.Application.SalesReturn.Queries.GetSalesReturnById;

namespace SalesManagement.UnitTests.Application.SalesReturn.Queries;

public sealed class GetSalesReturnByIdQueryHandlerTests
{
    private readonly Mock<ISalesReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetSalesReturnByIdQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingId_ReturnsSuccess()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new SalesReturnHeaderDto { Id = 1, ReturnNumber = "SR001" });

        var result = await CreateSut().Handle(
            new GetSalesReturnByIdQuery { Id = 1 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsNotFound()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((SalesReturnHeaderDto?)null);

        var result = await CreateSut().Handle(
            new GetSalesReturnByIdQuery { Id = 99 }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }
}
