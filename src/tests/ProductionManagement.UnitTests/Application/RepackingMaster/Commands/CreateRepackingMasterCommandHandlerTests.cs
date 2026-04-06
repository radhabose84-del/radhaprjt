using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Commands.CreateRepackingMaster;

namespace ProductionManagement.UnitTests.Application.RepackingMaster.Commands
{
    public sealed class CreateRepackingMasterCommandHandlerTests
    {
        private readonly Mock<IRepackingMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSequence = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private CreateRepackingMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockDocSequence.Object, _mockMediator.Object, _mockMapper.Object, _mockIpService.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockMapper
                .Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingMaster>(It.IsAny<CreateRepackingMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingMaster());

            _mockDocSequence
                .Setup(d => d.GetTransactionTypeIdAsync(
                    ProductionManagement.Domain.Common.MiscEnumEntity.TransactionTypeRePackMaster,
                    ProductionManagement.Domain.Common.MiscEnumEntity.ModuleSales,
                    It.IsAny<int>()))
                .ReturnsAsync(10);

            _mockDocSequence
                .Setup(d => d.GenerateDocumentNumber(10))
                .ReturnsAsync(new List<string> { "RPK-001" } as IReadOnlyList<string>);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.RepackingMaster>(), 10))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new CreateRepackingMasterCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(new CreateRepackingMasterCommand(), CancellationToken.None);
            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateRepackingMasterCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.RepackingMaster>(), 10),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateRepackingMasterCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "REPACKING_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_TransactionTypeNotFound_ThrowsExceptionRules()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper
                .Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingMaster>(It.IsAny<CreateRepackingMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingMaster());
            _mockDocSequence
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(new CreateRepackingMasterCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Transaction Type*");
        }

        [Fact]
        public async Task Handle_NoDocumentSequence_ThrowsExceptionRules()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper
                .Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingMaster>(It.IsAny<CreateRepackingMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingMaster());
            _mockDocSequence
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(10);
            _mockDocSequence
                .Setup(d => d.GenerateDocumentNumber(10))
                .ReturnsAsync(new List<string>() as IReadOnlyList<string>);

            Func<Task> act = async () => await CreateSut().Handle(new CreateRepackingMasterCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*document sequence*");
        }

        [Fact]
        public async Task Handle_TransactionTypeNotFound_DoesNotPublishAuditEvent()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper
                .Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingMaster>(It.IsAny<CreateRepackingMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingMaster());
            _mockDocSequence
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((int?)null);

            try { await CreateSut().Handle(new CreateRepackingMasterCommand(), CancellationToken.None); }
            catch { /* expected */ }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
