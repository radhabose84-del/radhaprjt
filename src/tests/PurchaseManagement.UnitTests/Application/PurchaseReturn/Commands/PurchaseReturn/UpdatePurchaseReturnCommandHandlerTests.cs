using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.UpdatePurchaseReturn;
using PurchaseManagement.Domain.Entities.PurchaseReturn;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Commands.PurchaseReturn;

public sealed class UpdatePurchaseReturnCommandHandlerTests
{
    private readonly Mock<IPurchaseReturnCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
    private readonly Mock<IPurchaseReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private UpdatePurchaseReturnCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_DraftStatus_ReturnsUpdated()
    {
        _mockQueryRepo.Setup(r => r.GetCurrentStatusCodeAsync(1)).ReturnsAsync("Draft");
        var entity = PurchaseReturnBuilders.ValidHeaderEntity();
        _mockMapper.Setup(m => m.Map<PurchaseReturnHeader>(It.IsAny<UpdatePurchaseReturnCommand>())).Returns(entity);
        _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<PurchaseReturnHeader>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(PurchaseReturnBuilders.ValidHeaderDto());

        var result = await CreateSut().Handle(PurchaseReturnBuilders.ValidUpdateCommand(), CancellationToken.None);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NotDraft_ThrowsExceptionRules()
    {
        _mockQueryRepo.Setup(r => r.GetCurrentStatusCodeAsync(1)).ReturnsAsync("Approved");

        Func<Task> act = async () => await CreateSut().Handle(PurchaseReturnBuilders.ValidUpdateCommand(), CancellationToken.None);
        await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*Draft*");
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsExceptionRules()
    {
        _mockQueryRepo.Setup(r => r.GetCurrentStatusCodeAsync(99)).ReturnsAsync((string?)null);

        Func<Task> act = async () => await CreateSut().Handle(PurchaseReturnBuilders.ValidUpdateCommand(99), CancellationToken.None);
        await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*not found*");
    }
}
