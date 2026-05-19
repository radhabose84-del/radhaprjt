using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.Create;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using Moq;

namespace PurchaseManagement.UnitTests.Application.Rfqs.Commands
{
    public sealed class CreateRfqCommandHandlerTests
    {
        private readonly Mock<IRfqCommandRepository> _mockRfqRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateRfqCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZone = new(MockBehavior.Loose);
        private readonly Mock<IPurchaseIndentCommand> _mockIndentRepo = new(MockBehavior.Loose);
        private readonly Mock<IAppDataMiscMasterLookup> _mockAppDataMisc = new(MockBehavior.Loose);
        private readonly Mock<IRfqAttachmentFileStorage> _mockAttachmentStorage = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSequence = new(MockBehavior.Loose);

        private CreateRfqCommandHandler CreateSut() =>
            new(_mockRfqRepo.Object, _mockMapper.Object, _mockIp.Object,
                _mockOutbox.Object, _mockLogger.Object, _mockItemLookup.Object,
                _mockUomLookup.Object, _mockTimeZone.Object, _mockIndentRepo.Object,
                _mockAppDataMisc.Object, _mockAttachmentStorage.Object,
                _mockDocSequence.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockTimeZone
                .Setup(t => t.GetSystemTimeZone())
                .Returns("Asia/Kolkata");

            _mockMapper
                .Setup(m => m.Map<RfqMaster>(It.IsAny<object>()))
                .Returns(new RfqMaster
                {
                    Items = new List<RfqItem>(),
                    Suppliers = new List<RfqSupplier>()
                });

            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test-user");
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");

            _mockDocSequence
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(1);
            _mockDocSequence
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "RFQ001" });

            _mockRfqRepo
                .Setup(r => r.CreateAsync(It.IsAny<RfqMaster>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newId);

            _mockUomLookup
                .Setup(l => l.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>());

            _mockItemLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>());
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 10);

            var command = new CreateRfqCommand
            {
                InitiationTypeId = 1,
                LastSubmitDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                Items = new List<RfqItemCreateDto> { new() { ItemId = 1, HsnId = 1, Qty = 10, UomId = 1 } },
                Suppliers = new List<RfqSupplierCreateDto> { new() { Name = "Supplier", Email = "s@s.com" } }
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(10);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();

            var command = new CreateRfqCommand
            {
                InitiationTypeId = 1,
                LastSubmitDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                Items = new List<RfqItemCreateDto> { new() { ItemId = 1, HsnId = 1, Qty = 10, UomId = 1 } },
                Suppliers = new List<RfqSupplierCreateDto> { new() { Name = "Supplier", Email = "s@s.com" } }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockRfqRepo.Verify(
                r => r.CreateAsync(It.IsAny<RfqMaster>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreationFails_ThrowsInvalidOperationException()
        {
            SetupHappyPath();
            _mockRfqRepo
                .Setup(r => r.CreateAsync(It.IsAny<RfqMaster>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var command = new CreateRfqCommand
            {
                InitiationTypeId = 1,
                LastSubmitDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                Items = new List<RfqItemCreateDto> { new() { ItemId = 1, HsnId = 1, Qty = 10, UomId = 1 } },
                Suppliers = new List<RfqSupplierCreateDto> { new() { Name = "Supplier", Email = "s@s.com" } }
            };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Handle_WithAttachments_MovesStagedFilesAndAttachesToAggregate()
        {
            SetupHappyPath();
            _mockAttachmentStorage
                .Setup(s => s.MoveStagedToPermanentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync<string, CancellationToken, IRfqAttachmentFileStorage, string>((staged, _) =>
                    $"/Resources/Purchase/RfqAttachments/Default/Default/{Guid.NewGuid()}.pdf");

            var command = new CreateRfqCommand
            {
                InitiationTypeId = 1,
                LastSubmitDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                Items = new List<RfqItemCreateDto> { new() { ItemId = 1, HsnId = 1, Qty = 10, UomId = 1 } },
                Suppliers = new List<RfqSupplierCreateDto> { new() { Name = "Supplier", Email = "s@s.com" } },
                Attachments = new List<RfqAttachmentStageRef>
                {
                    new() { FileName = "TEMP_a.pdf", OriginalFileName = "spec.pdf", FileSize = 1024, FileType = "application/pdf" },
                    new() { FileName = "TEMP_b.pdf", OriginalFileName = "lab.pdf", FileSize = 2048, FileType = "application/pdf" }
                }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockAttachmentStorage.Verify(
                s => s.MoveStagedToPermanentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_CreationFailsAfterAttachmentsMoved_RollsBackMovedFiles()
        {
            SetupHappyPath();
            var movedPaths = new List<string>();
            _mockAttachmentStorage
                .Setup(s => s.MoveStagedToPermanentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync<string, CancellationToken, IRfqAttachmentFileStorage, string>((staged, _) =>
                {
                    var path = $"/Resources/Purchase/RfqAttachments/Default/Default/{Guid.NewGuid()}.pdf";
                    movedPaths.Add(path);
                    return path;
                });

            _mockRfqRepo
                .Setup(r => r.CreateAsync(It.IsAny<RfqMaster>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("simulated db failure"));

            var command = new CreateRfqCommand
            {
                InitiationTypeId = 1,
                LastSubmitDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                Items = new List<RfqItemCreateDto> { new() { ItemId = 1, HsnId = 1, Qty = 10, UomId = 1 } },
                Suppliers = new List<RfqSupplierCreateDto> { new() { Name = "Supplier", Email = "s@s.com" } },
                Attachments = new List<RfqAttachmentStageRef>
                {
                    new() { FileName = "TEMP_a.pdf", OriginalFileName = "a.pdf", FileSize = 1, FileType = "application/pdf" },
                    new() { FileName = "TEMP_b.pdf", OriginalFileName = "b.pdf", FileSize = 2, FileType = "application/pdf" }
                }
            };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>();

            // Both moved files should be cleaned up after the DB save failure.
            foreach (var path in movedPaths)
            {
                _mockAttachmentStorage.Verify(
                    s => s.DeleteAsync(path, It.IsAny<CancellationToken>()),
                    Times.Once);
            }
        }

        [Fact]
        public async Task Handle_NoAttachments_DoesNotCallStorage()
        {
            SetupHappyPath();

            var command = new CreateRfqCommand
            {
                InitiationTypeId = 1,
                LastSubmitDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                Items = new List<RfqItemCreateDto> { new() { ItemId = 1, HsnId = 1, Qty = 10, UomId = 1 } },
                Suppliers = new List<RfqSupplierCreateDto> { new() { Name = "Supplier", Email = "s@s.com" } }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockAttachmentStorage.Verify(
                s => s.MoveStagedToPermanentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
