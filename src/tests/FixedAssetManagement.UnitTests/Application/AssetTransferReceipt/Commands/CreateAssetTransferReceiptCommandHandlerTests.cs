using AutoMapper;
using Contracts.Interfaces;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.AssetMaster.AssetTransferReceipt.Command.CreateAssetTransferReceipt;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptPending;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferReceipt.Commands
{
    public sealed class CreateAssetTransferReceiptCommandHandlerTests
    {
        private readonly Mock<IAssetTransferReceiptCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetTransferReceiptQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZoneService = new(MockBehavior.Loose);

        private CreateAssetTransferReceiptCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object,
                _mockIpService.Object, _mockTimeZoneService.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockIpService.Setup(s => s.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockIpService.Setup(s => s.GetUserId()).Returns(1);
            _mockIpService.Setup(s => s.GetUserName()).Returns("test-user");
            _mockTimeZoneService.Setup(s => s.GetSystemTimeZone()).Returns("UTC");
            _mockTimeZoneService.Setup(s => s.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

            _mockQueryRepo
                .Setup(r => r.GetByAssetTransferId(It.IsAny<int>()))
                .ReturnsAsync(new AssetTransferDto
                {
                    ToCustodianId = 1,
                    ToUnitId = 1,
                    ToDepartmentId = 1
                });

            _mockQueryRepo
                .Setup(r => r.GetByAssetReceiptId(It.IsAny<int>()))
                .ReturnsAsync(new List<AssetReceiptDetailsByIdDto>());

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetMaster.AssetLocation>(It.IsAny<object>()))
                .Returns(new FAM.Domain.Entities.AssetMaster.AssetLocation());

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetMaster.AssetTransferReceiptHdr>(It.IsAny<object>()))
                .Returns(new FAM.Domain.Entities.AssetMaster.AssetTransferReceiptHdr
                {
                    Id = newId,
                    AssetTransferId = 1
                });

            _mockCommandRepo
                .Setup(r => r.CreateAsync(
                    It.IsAny<FAM.Domain.Entities.AssetMaster.AssetTransferReceiptHdr>(),
                    It.IsAny<List<FAM.Domain.Entities.AssetMaster.AssetLocation>>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(42);
            var command = new CreateAssetTransferReceiptCommand
            {
                AssetTransferReceiptHdrDto = new AssetTransferReceiptHdrDto
                {
                    AssetTransferId = 1,
                    DocDate = DateTimeOffset.UtcNow,
                    AssetTransferReceiptDtl = new List<AssetTransferReceiptDtlDto>()
                }
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            var command = new CreateAssetTransferReceiptCommand
            {
                AssetTransferReceiptHdrDto = new AssetTransferReceiptHdrDto
                {
                    AssetTransferId = 1,
                    DocDate = DateTimeOffset.UtcNow,
                    AssetTransferReceiptDtl = new List<AssetTransferReceiptDtlDto>()
                }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(
                It.IsAny<FAM.Domain.Entities.AssetMaster.AssetTransferReceiptHdr>(),
                It.IsAny<List<FAM.Domain.Entities.AssetMaster.AssetLocation>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = new CreateAssetTransferReceiptCommand
            {
                AssetTransferReceiptHdrDto = new AssetTransferReceiptHdrDto
                {
                    AssetTransferId = 1,
                    DocDate = DateTimeOffset.UtcNow,
                    AssetTransferReceiptDtl = new List<AssetTransferReceiptDtlDto>()
                }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            SetupHappyPath(1);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(
                    It.IsAny<FAM.Domain.Entities.AssetMaster.AssetTransferReceiptHdr>(),
                    It.IsAny<List<FAM.Domain.Entities.AssetMaster.AssetLocation>>()))
                .ReturnsAsync(0);

            var command = new CreateAssetTransferReceiptCommand
            {
                AssetTransferReceiptHdrDto = new AssetTransferReceiptHdrDto
                {
                    AssetTransferId = 1,
                    DocDate = DateTimeOffset.UtcNow,
                    AssetTransferReceiptDtl = new List<AssetTransferReceiptDtlDto>()
                }
            };

            await Assert.ThrowsAsync<Exception>(() =>
                CreateSut().Handle(command, CancellationToken.None));
        }
    }
}
