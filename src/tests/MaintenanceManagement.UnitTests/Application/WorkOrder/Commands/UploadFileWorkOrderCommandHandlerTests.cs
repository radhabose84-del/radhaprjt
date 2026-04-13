using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Commands
{
    public sealed class UploadFileWorkOrderCommandHandlerTests
    {
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UploadFileWorkOrderCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);

        private UploadFileWorkOrderCommandHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockLogger.Object, _mockIp.Object,
                _mockCommandRepo.Object, _mockUnitLookup.Object, _mockCompanyLookup.Object);

        [Fact]
        public async Task Handle_NullFile_ReturnsFailure()
        {
            var result = await CreateSut().Handle(
                new UploadFileWorkOrderCommand { File = null }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("No file");
        }

        [Fact]
        public async Task Handle_EmptyFile_ReturnsFailure()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            var result = await CreateSut().Handle(
                new UploadFileWorkOrderCommand { File = mockFile.Object }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("No file");
        }

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ReturnsFailure()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("test.jpg");

            _mockQueryRepo.Setup(q => q.GetBaseDirectoryAsync()).ReturnsAsync(string.Empty);

            var result = await CreateSut().Handle(
                new UploadFileWorkOrderCommand { File = mockFile.Object }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Base directory");
        }

        [Fact]
        public async Task Handle_NullFile_DoesNotCallRepository()
        {
            await CreateSut().Handle(
                new UploadFileWorkOrderCommand { File = null }, CancellationToken.None);

            _mockQueryRepo.Verify(q => q.GetBaseDirectoryAsync(), Times.Never);
        }
    }
}
