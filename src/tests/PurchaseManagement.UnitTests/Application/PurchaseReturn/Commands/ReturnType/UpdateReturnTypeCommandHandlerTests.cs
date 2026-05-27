using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.UpdateReturnType;
using PurchaseManagement.UnitTests.TestData;
using DomainReturnType = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnType;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Commands.ReturnType;

public sealed class UpdateReturnTypeCommandHandlerTests
{
    private readonly Mock<IReturnTypeCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
    private readonly Mock<IReturnTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private UpdateReturnTypeCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsDto()
    {
        var entity = ReturnTypeBuilders.ValidEntity();
        _mockMapper.Setup(m => m.Map<DomainReturnType>(It.IsAny<UpdateReturnTypeCommand>())).Returns(entity);
        _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<DomainReturnType>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _mockQueryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(ReturnTypeBuilders.ValidDto());

        var result = await CreateSut().Handle(ReturnTypeBuilders.ValidUpdateCommand(), CancellationToken.None);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUpdateOnce()
    {
        var entity = ReturnTypeBuilders.ValidEntity();
        _mockMapper.Setup(m => m.Map<DomainReturnType>(It.IsAny<UpdateReturnTypeCommand>())).Returns(entity);
        _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<DomainReturnType>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _mockQueryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(ReturnTypeBuilders.ValidDto());

        await CreateSut().Handle(ReturnTypeBuilders.ValidUpdateCommand(), CancellationToken.None);

        _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<DomainReturnType>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
