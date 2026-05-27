using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.CreateReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;
using PurchaseManagement.UnitTests.TestData;
using DomainReturnType = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnType;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Commands.ReturnType;

public sealed class CreateReturnTypeCommandHandlerTests
{
    private readonly Mock<IReturnTypeCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
    private readonly Mock<IReturnTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private CreateReturnTypeCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    private void SetupHappyPath(int id = 1)
    {
        var entity = ReturnTypeBuilders.ValidEntity(id);
        _mockMapper.Setup(m => m.Map<DomainReturnType>(It.IsAny<CreateReturnTypeCommand>())).Returns(entity);
        _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<DomainReturnType>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _mockQueryRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(ReturnTypeBuilders.ValidDto(id));
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsDto()
    {
        SetupHappyPath();
        var result = await CreateSut().Handle(ReturnTypeBuilders.ValidCreateCommand(), CancellationToken.None);
        result.Should().NotBeNull();
        result.Code.Should().Be("Rejected");
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateOnce()
    {
        SetupHappyPath();
        await CreateSut().Handle(ReturnTypeBuilders.ValidCreateCommand(), CancellationToken.None);
        _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<DomainReturnType>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath();
        await CreateSut().Handle(ReturnTypeBuilders.ValidCreateCommand(), CancellationToken.None);
        _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
