using AutoMapper;
using Contracts.Interfaces;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Application.ExcelImport.PhysicalStockVerification;
using FAM.Domain.Entities.AssetMaster;

namespace FixedAssetManagement.UnitTests.Application.ExcelImport.PhysicalStockVerification
{
    public sealed class ScanAssetAuditCommandHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IExcelImportCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);

        private ScanAssetAuditCommandHandler CreateSut()
        {
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test-user");
            _mockTz.Setup(t => t.GetSystemTimeZone()).Returns("UTC");
            _mockTz.Setup(t => t.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

            return new ScanAssetAuditCommandHandler(
                _mockMapper.Object,
                _mockIp.Object,
                _mockCommandRepo.Object,
                _mockTz.Object);
        }

        private static ScanAssetAuditCommand ValidCommand() => new()
        {
            AssetCode = "AC001",
            AuditCycle = 1,
            DepartmentName = "IT",
            UnitName = "U1"
        };

        [Fact]
        public async Task Handle_WhenAlreadyScanned_ReturnsFailure()
        {
            _mockCommandRepo
                .Setup(r => r.IsAssetAlreadyScannedAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("already scanned");
        }

        [Fact]
        public async Task Handle_WhenNotScannedAndInsertSucceeds_ReturnsSuccess()
        {
            _mockCommandRepo
                .Setup(r => r.IsAssetAlreadyScannedAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.InsertScannedAssetAsync(It.IsAny<AssetAudit>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Scanned asset inserted successfully.");
        }

        [Fact]
        public async Task Handle_WhenInsertFails_ReturnsFailure()
        {
            _mockCommandRepo
                .Setup(r => r.IsAssetAlreadyScannedAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.InsertScannedAssetAsync(It.IsAny<AssetAudit>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Failed to insert scanned asset.");
        }

        [Fact]
        public async Task Handle_WhenSuccessful_CallsInsertOnce()
        {
            _mockCommandRepo
                .Setup(r => r.IsAssetAlreadyScannedAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.InsertScannedAssetAsync(It.IsAny<AssetAudit>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.InsertScannedAssetAsync(It.IsAny<AssetAudit>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
