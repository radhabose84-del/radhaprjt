using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder.Item;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Commands
{
    public sealed class UploadFileItemCommandHandlerTests
    {
        private readonly Mock<ILogger<UploadFileWorkOrderCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);

        private UploadFileItemCommandHandler CreateSut() =>
            new(_mockLogger.Object, _mockIp.Object, _mockCommandRepo.Object,
                _mockUnitLookup.Object, _mockCompanyLookup.Object);

        [Fact]
        public async Task Handle_NullFile_ReturnsFailure()
        {
            var result = await CreateSut().Handle(
                new UploadFileItemCommand { File = null }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("No file");
        }

        [Fact]
        public async Task Handle_EmptyFile_ReturnsFailure()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            var result = await CreateSut().Handle(
                new UploadFileItemCommand { File = mockFile.Object }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("No file");
        }

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ReturnsFailure()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("test.jpg");

            _mockCommandRepo.Setup(r => r.GetBaseDirectoryItemAsync()).ReturnsAsync(string.Empty);

            var result = await CreateSut().Handle(
                new UploadFileItemCommand { File = mockFile.Object }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Base directory");
        }

        [Fact]
        public async Task Handle_NullFile_DoesNotCallRepository()
        {
            await CreateSut().Handle(
                new UploadFileItemCommand { File = null }, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.GetBaseDirectoryItemAsync(), Times.Never);
        }
    }
}
