using Contracts.Interfaces;
using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Application.LotMaster.Commands.CreateLotMaster;

namespace ProductionManagement.UnitTests.Application.LotMaster.Commands
{
    public sealed class CreateLotMasterCommandHandlerTests
    {
        private readonly Mock<ILotMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ILotMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateLotMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object,
                _mockMapper.Object, _mockIp.Object);

        private void SetupHappyPath(int newId = 1, int unitId = 1)
        {
            _mockIp.Setup(x => x.GetUnitId()).Returns(unitId);
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.LotMaster>(It.IsAny<CreateLotMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.LotMaster());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.LotMaster>()))
                .ReturnsAsync(newId);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new CreateLotMasterCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(77);
            var result = await CreateSut().Handle(new CreateLotMasterCommand(), CancellationToken.None);
            result.Data.Should().Be(77);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateLotMasterCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.LotMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateLotMasterCommand(), CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_StampsUnitIdFromToken()
        {
            ProductionManagement.Domain.Entities.LotMaster? captured = null;
            _mockIp.Setup(x => x.GetUnitId()).Returns(7);
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.LotMaster>(It.IsAny<CreateLotMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.LotMaster());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.LotMaster>()))
                .Callback<ProductionManagement.Domain.Entities.LotMaster>(e => captured = e)
                .ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new CreateLotMasterCommand(), CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.UnitId.Should().Be(7);
        }

        [Fact]
        public async Task Handle_NoUnitClaim_StampsZeroUnitId()
        {
            ProductionManagement.Domain.Entities.LotMaster? captured = null;
            _mockIp.Setup(x => x.GetUnitId()).Returns((int?)null);
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.LotMaster>(It.IsAny<CreateLotMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.LotMaster());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.LotMaster>()))
                .Callback<ProductionManagement.Domain.Entities.LotMaster>(e => captured = e)
                .ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new CreateLotMasterCommand(), CancellationToken.None);

            captured!.UnitId.Should().Be(0);
        }
    }
}
