using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.CreateReturnReason;
using PurchaseManagement.UnitTests.TestData;
using DomainReturnReason = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnReason;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Commands.ReturnReason;

public sealed class CreateReturnReasonCommandHandlerTests
{
    private readonly Mock<IReturnReasonCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
    private readonly Mock<IReturnReasonQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private CreateReturnReasonCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsDto()
    {
        var entity = ReturnReasonBuilders.ValidEntity();
        _mockMapper.Setup(m => m.Map<DomainReturnReason>(It.IsAny<CreateReturnReasonCommand>())).Returns(entity);
        _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<DomainReturnReason>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _mockQueryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(ReturnReasonBuilders.ValidDto());

        var result = await CreateSut().Handle(ReturnReasonBuilders.ValidCreateCommand(), CancellationToken.None);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        _mockMapper.Setup(m => m.Map<DomainReturnReason>(It.IsAny<CreateReturnReasonCommand>())).Returns(ReturnReasonBuilders.ValidEntity());
        _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<DomainReturnReason>(), It.IsAny<CancellationToken>())).ReturnsAsync(ReturnReasonBuilders.ValidEntity());
        _mockQueryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(ReturnReasonBuilders.ValidDto());

        await CreateSut().Handle(ReturnReasonBuilders.ValidCreateCommand(), CancellationToken.None);

        _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
