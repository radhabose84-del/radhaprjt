using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.CreateRepackingHeader;
using ProductionManagement.Domain.Common;

namespace ProductionManagement.UnitTests.Application.Repacking.Commands
{
    public sealed class CreateRepackingHeaderCommandHandlerTests
    {
        private readonly Mock<IRepackingHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private CreateRepackingHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockDocSeq.Object, _mockMediator.Object, _mockMapper.Object, _mockIpService.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingHeader>(It.IsAny<CreateRepackingHeaderCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingHeader());

            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(
                    MiscEnumEntity.TransactionTypeRePackMaster, MiscEnumEntity.ModuleSales, 1))
                .ReturnsAsync(6);

            _mockDocSeq.Setup(d => d.GenerateDocumentNumber(6))
                .ReturnsAsync(new List<string> { "REPACK-001" });

            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.RepackingHeader>(), 6))
                .ReturnsAsync(newId);

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        private static CreateRepackingHeaderCommand BuildValidCommand() => new()
        {
            RepackDate = DateOnly.FromDateTime(DateTime.Today),
            ItemId = 1,
            OldItemId = 1,
            OldPackTypeId = 1,
            PackTypeId = 2,
            NetWeightPerPack = 25m,
            TotalBags = 20,
            NetWeight = 500m,
            WarehouseId = 2,
            BinId = 2,
            LooseConeKgs = 0m,
            Remarks = "Test repack",
            Details = new List<CreateRepackingDetailItem>
            {
                new() { OldStartPackNo = 1, OldEndPackNo = 10 }
            }
        };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(99);
            var result = await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            result.Data.Should().Be(99);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.RepackingHeader>(), 6), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "REPACKING_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsRepackDocNo()
        {
            SetupHappyPath();
            ProductionManagement.Domain.Entities.RepackingHeader? capturedEntity = null;

            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingHeader>(It.IsAny<CreateRepackingHeaderCommand>()))
                .Returns(() =>
                {
                    capturedEntity = new ProductionManagement.Domain.Entities.RepackingHeader();
                    return capturedEntity;
                });

            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.RepackingHeader>(), 6))
                .ReturnsAsync(1);

            await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.RepackDocNo.Should().Be("REPACK-001");
        }

        [Fact]
        public async Task Handle_NoTransactionTypeId_ThrowsException()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingHeader>(It.IsAny<CreateRepackingHeaderCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingHeader());

            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(
                    MiscEnumEntity.TransactionTypeRePackMaster, MiscEnumEntity.ModuleSales, 1))
                .ReturnsAsync((int?)null);

            Func<Task> act = () => CreateSut().Handle(BuildValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Transaction Type*not found*");
        }

        [Fact]
        public async Task Handle_EmptyDocumentSequence_ThrowsException()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingHeader>(It.IsAny<CreateRepackingHeaderCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingHeader());

            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(
                    MiscEnumEntity.TransactionTypeRePackMaster, MiscEnumEntity.ModuleSales, 1))
                .ReturnsAsync(6);
            _mockDocSeq.Setup(d => d.GenerateDocumentNumber(6))
                .ReturnsAsync(new List<string>());

            Func<Task> act = () => CreateSut().Handle(BuildValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*No document sequence configured*");
        }
    }
}
