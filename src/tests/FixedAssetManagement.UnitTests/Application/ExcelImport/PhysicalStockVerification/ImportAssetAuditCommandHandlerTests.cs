using AutoMapper;
using Contracts.Interfaces;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Application.ExcelImport.PhysicalStockVerification;
using Microsoft.AspNetCore.Http;

namespace FixedAssetManagement.UnitTests.Application.ExcelImport.PhysicalStockVerification
{
    public sealed class ImportAssetAuditCommandHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IExcelImportCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);

        private ImportAssetAuditCommandHandler CreateSut()
        {
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test");
            _mockTz.Setup(t => t.GetSystemTimeZone()).Returns("UTC");
            _mockTz.Setup(t => t.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

            return new ImportAssetAuditCommandHandler(
                _mockMapper.Object,
                _mockIp.Object,
                _mockCommandRepo.Object,
                _mockTz.Object);
        }

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_WhenFileIsNull_ReturnsFailure()
        {
            var command = new ImportAssetAuditCommand(new ImportAssetAuditDto { File = null });

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Invalid file uploaded");
        }

        [Fact]
        public async Task Handle_WhenFileLengthIsZero_ReturnsFailure()
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Loose);
            fileMock.Setup(f => f.Length).Returns(0);

            var command = new ImportAssetAuditCommand(new ImportAssetAuditDto { File = fileMock.Object });
            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Invalid file uploaded");
        }

        [Fact]
        public async Task Handle_WhenFileExceeds2MB_ReturnsFailure()
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Loose);
            fileMock.Setup(f => f.Length).Returns(3 * 1024 * 1024);
            fileMock.Setup(f => f.FileName).Returns("big.xlsx");

            var command = new ImportAssetAuditCommand(new ImportAssetAuditDto { File = fileMock.Object });
            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("File size exceeds 2MB");
        }

        [Fact]
        public async Task Handle_WhenFileAlreadyExists_ReturnsFailure()
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Loose);
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("audit.xlsx");

            _mockCommandRepo
                .Setup(r => r.CheckFileExistsAsync("audit.xlsx", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new ImportAssetAuditCommand(new ImportAssetAuditDto { File = fileMock.Object });
            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("already been uploaded");
        }
    }
}
