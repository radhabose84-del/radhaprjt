using AutoMapper;
using Contracts.Interfaces;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IMiscMaster;
using FAM.Application.ExcelImport.MiscMaster;
using Microsoft.AspNetCore.Http;

namespace FixedAssetManagement.UnitTests.Application.ExcelImport.MiscMaster
{
    public sealed class MiscMasterImportCommandHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);

        private MiscMasterImportCommandHandler CreateSut()
        {
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test");
            _mockTz.Setup(t => t.GetSystemTimeZone()).Returns("UTC");
            _mockTz.Setup(t => t.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

            return new MiscMasterImportCommandHandler(
                _mockMapper.Object,
                _mockIp.Object,
                _mockTz.Object,
                _mockQueryRepo.Object,
                _mockCommandRepo.Object);
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
            var command = new MiscMasterImportCommand(null!);
            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Invalid file uploaded");
        }

        [Fact]
        public async Task Handle_WhenFileLengthIsZero_ReturnsFailure()
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Loose);
            fileMock.Setup(f => f.Length).Returns(0);

            var command = new MiscMasterImportCommand(fileMock.Object);
            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Invalid file uploaded");
        }

        [Fact]
        public async Task Handle_WhenFileExceeds2MB_ReturnsFailure()
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Loose);
            fileMock.Setup(f => f.Length).Returns(3 * 1024 * 1024);

            var command = new MiscMasterImportCommand(fileMock.Object);
            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("File size exceeds 2MB");
        }
    }
}
