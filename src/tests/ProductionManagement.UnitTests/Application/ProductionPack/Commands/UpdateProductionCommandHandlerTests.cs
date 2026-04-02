using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Commands.UpdateProduction;
using Contracts.Interfaces;
using ProductionManagement.Domain.Entities;
using ProductionManagement.Application.ProductionPack.Dto;

namespace ProductionManagement.UnitTests.Application.ProductionPack.Commands
{
    public sealed class UpdateProductionCommandHandlerTests
    {
        private readonly Mock<IProductionCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IProductionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private UpdateProductionCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockIpService.Object);

        private void SetupHappyPath()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper.Setup(m => m.Map<ProductionPackHeader>(It.IsAny<object>()))
                .Returns(new ProductionPackHeader());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductionPackHeader>())).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new UpdateProductionCommand { ProductionPackDetails = new UpdateProductionDto() }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateProductionCommand { ProductionPackDetails = new UpdateProductionDto() }, CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<ProductionPackHeader>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateProductionCommand { ProductionPackDetails = new UpdateProductionDto() }, CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
