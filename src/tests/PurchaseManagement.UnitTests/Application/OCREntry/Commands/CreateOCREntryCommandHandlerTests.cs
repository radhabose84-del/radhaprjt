using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.OCREntry.Commands.CreateOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.OCREntry.Commands
{
    public sealed class CreateOCREntryCommandHandlerTests
    {
        private readonly Mock<IOCREntryCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IOCREntryQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);
        private readonly Mock<IOCREntryFileStorage> _mockFileStorage = new(MockBehavior.Loose);

        private CreateOCREntryCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object,
                _mockDocSeq.Object, _mockIp.Object, _mockOutbox.Object, _mockMisc.Object, _mockFileStorage.Object);

        private void SetupHappyPath(int newId = 1, string generatedNumber = "OCR-2026-0001")
        {
            _mockMisc
                .Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 9 });
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockDocSeq
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(1);
            _mockDocSeq
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { generatedNumber });
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.OCREntry>(It.IsAny<object>()))
                .Returns(OCREntryBuilders.ValidEntity(newId));
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.OCREntry>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(OCREntryBuilders.ValidEntity(newId));
            _mockCommandRepo
                .Setup(r => r.GetByIdOCRWorkFlowAsync(It.IsAny<int>()))
                .ReturnsAsync(new OCREntryWorkFlowDto { Id = newId, OcrNumber = generatedNumber });
            _mockOutbox
                .Setup(o => o.ScheduleAsync(It.IsAny<CreateApprovalRequestCommand>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(OCREntryBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(OCREntryBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(OCREntryBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.OCREntry>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_GeneratesOcrNumber()
        {
            SetupHappyPath();
            await CreateSut().Handle(OCREntryBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockDocSeq.Verify(d => d.GenerateDocumentNumber(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SchedulesApprovalRequest()
        {
            SetupHappyPath();
            await CreateSut().Handle(OCREntryBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockOutbox.Verify(
                o => o.ScheduleAsync(It.IsAny<CreateApprovalRequestCommand>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(OCREntryBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UploadedDocument_RenamedToOcrNumber()
        {
            SetupHappyPath(generatedNumber: "OCR-2026-0001");
            var entity = OCREntryBuilders.ValidEntity();
            entity.DocumentPath = "TEMP_abc.png"; // temp name returned by upload
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.OCREntry>(It.IsAny<object>()))
                .Returns(entity);
            _mockFileStorage
                .Setup(s => s.RenameAsync("TEMP_abc.png", "OCR-2026-0001", It.IsAny<CancellationToken>()))
                .ReturnsAsync("OCR-2026-0001.png");

            await CreateSut().Handle(OCREntryBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockFileStorage.Verify(
                s => s.RenameAsync("TEMP_abc.png", "OCR-2026-0001", It.IsAny<CancellationToken>()),
                Times.Once);
            entity.DocumentPath.Should().Be("OCR-2026-0001.png");
        }

        [Fact]
        public async Task Handle_NoDocument_DoesNotRename()
        {
            SetupHappyPath();
            // ValidEntity has a null DocumentPath — nothing to rename.
            await CreateSut().Handle(OCREntryBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockFileStorage.Verify(
                s => s.RenameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
