using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Commands.CreateProduction;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using ProductionManagement.Domain.Entities;
using ProductionManagement.Application.ProductionPack.Dto;

namespace ProductionManagement.UnitTests.Application.ProductionPack.Commands
{
    public sealed class CreateProductionCommandHandlerTests
    {
        private readonly Mock<IProductionCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private CreateProductionCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockDocSeq.Object, _mockMediator.Object, _mockMapper.Object, _mockIpService.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper.Setup(m => m.Map<ProductionPackHeader>(It.IsAny<object>()))
                .Returns(new ProductionPackHeader());
            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(5);
            _mockDocSeq.Setup(d => d.GenerateDocumentNumber(5))
                .ReturnsAsync(new List<string> { "PACK-001" });
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<ProductionPackHeader>(), 5))
                .ReturnsAsync(newId);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var cmd = new CreateProductionCommand { ProductionPackDetails = new CreateProductionDto() };
            var result = await CreateSut().Handle(cmd, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(55);
            var cmd = new CreateProductionCommand { ProductionPackDetails = new CreateProductionDto() };
            var result = await CreateSut().Handle(cmd, CancellationToken.None);
            result.Data.Should().Be(55);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            var cmd = new CreateProductionCommand { ProductionPackDetails = new CreateProductionDto() };
            await CreateSut().Handle(cmd, CancellationToken.None);
            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<ProductionPackHeader>(), 5), Times.Once);
        }
    }
}
