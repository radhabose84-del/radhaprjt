using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Purchase;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Commands.CreateGateInward;
using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Entities;
using GateEntryManagement.Domain.Events;
using GateEntryManagement.UnitTests.TestData;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.GateInward.Commands
{
    public sealed class CreateGateInwardCommandHandlerTests
    {
        private readonly Mock<IGateInwardCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IGateInwardQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Loose);
        private readonly Mock<ITransactionTypeLookup> _mockTransactionTypeLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IGateInwardAttachmentFileStorage> _mockStorage = new(MockBehavior.Loose);
        private readonly Mock<IGateInwardGrnBridge> _mockGrnBridge = new(MockBehavior.Loose);

        private CreateGateInwardCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockDocSeqLookup.Object,
                _mockTransactionTypeLookup.Object, _mockMediator.Object, _mockMapper.Object,
                _mockIpService.Object, _mockStorage.Object, _mockGrnBridge.Object);

        private void SetupHappyPath()
        {
            _mockMapper.Setup(m => m.Map<GateInwardHdr>(It.IsAny<object>())).Returns(new GateInwardHdr());
            _mockMapper.Setup(m => m.Map<List<GateInwardDtl>>(It.IsAny<object>())).Returns(new List<GateInwardDtl>());
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockDocSeqLookup
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(5);
            _mockDocSeqLookup
                .Setup(d => d.GenerateDocumentNumber(5))
                .ReturnsAsync((IReadOnlyList<string>)new List<string> { "GE00001" });
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<GateInwardHdr>(), It.IsAny<int>()))
                .ReturnsAsync(42);
            _mockQueryRepo
                .Setup(r => r.GetDocumentDirectoryPath())
                .ReturnsAsync(new Dictionary<string, string>
                {
                    { MiscEnumEntity.GateEntryImage, "GateEntry" },
                    { MiscEnumEntity.ImagePath, "http://192.168.1.126/Resources/" }
                });
            _mockStorage
                .Setup(s => s.MoveStagedToPermanentAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("GateEntry/GE00001.pdf");
        }

        [Fact]
        public void Constructor_AllDependencies_CreatesHandler()
        {
            CreateSut().Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessWithNewId()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(GateInwardBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_NoAttachment_DoesNotCallStorage()
        {
            SetupHappyPath();
            await CreateSut().Handle(GateInwardBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockStorage.Verify(
                s => s.MoveStagedToPermanentAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WithAttachment_MovesStagedAndSetsEntityColumns()
        {
            SetupHappyPath();
            GateInwardHdr? captured = null;
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<GateInwardHdr>(), It.IsAny<int>()))
                .Callback<GateInwardHdr, int>((e, _) => captured = e)
                .ReturnsAsync(7);

            await CreateSut().Handle(GateInwardBuilders.ValidCreateCommandWithAttachment(), CancellationToken.None);

            _mockStorage.Verify(
                s => s.MoveStagedToPermanentAsync("TEMP_abc.pdf", "GateEntry", "GE00001", It.IsAny<CancellationToken>()),
                Times.Once);
            captured.Should().NotBeNull();
            captured!.AttachmentFileName.Should().Be("GE00001.pdf");
            captured.AttachmentFilePath.Should().Be("GateEntry/GE00001.pdf");
        }

        [Fact]
        public async Task Handle_WhenCreateFails_DeletesMovedAttachmentFile()
        {
            SetupHappyPath();
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<GateInwardHdr>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("db failure"));

            var act = async () => await CreateSut().Handle(
                GateInwardBuilders.ValidCreateCommandWithAttachment(), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
            _mockStorage.Verify(
                s => s.DeleteAsync("GateEntry/GE00001.pdf", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(GateInwardBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GATEINWARD_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
