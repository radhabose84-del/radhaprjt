using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.DeleteFileWorkOrder;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Commands
{
    public sealed class DeleteFileWorkOrderCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileService = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ILogger<DeleteFileWorkOrderCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);

        private DeleteFileWorkOrderCommandHandler CreateSut() =>
            new(_mockFileService.Object, _mockQueryRepo.Object, _mockLogger.Object, _mockIp.Object,
                _mockCommandRepo.Object, _mockUnitLookup.Object, _mockCompanyLookup.Object);

        private void SetupMocks(string baseDirectory = "Uploads", bool deleteResult = true)
        {
            _mockIp.Setup(i => i.GetCompanyId()).Returns(1);
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);

            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto>
                {
                    new() { CompanyId = 1, CompanyName = "TestCo" }
                });

            _mockUnitLookup.Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new() { UnitId = 1, UnitName = "TestUnit" }
                });

            _mockQueryRepo.Setup(q => q.GetBaseDirectoryAsync()).ReturnsAsync(baseDirectory);
            _mockFileService.Setup(f => f.DeleteFileAsync(It.IsAny<string>())).ReturnsAsync(deleteResult);
            _mockCommandRepo.Setup(r => r.DeleteWOImageAsync(It.IsAny<string>())).ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ReturnsFailure()
        {
            SetupMocks(baseDirectory: "");

            var result = await CreateSut().Handle(
                new DeleteFileWorkOrderCommand { Image = "test.jpg" }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Base directory");
        }

        [Fact]
        public async Task Handle_DeleteSucceeds_ReturnsSuccess()
        {
            SetupMocks(deleteResult: true);

            var result = await CreateSut().Handle(
                new DeleteFileWorkOrderCommand { Image = "test.jpg" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DeleteFails_ReturnsFailure()
        {
            SetupMocks(deleteResult: false);

            var result = await CreateSut().Handle(
                new DeleteFileWorkOrderCommand { Image = "test.jpg" }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_CallsDeleteWOImageAsyncOnce()
        {
            SetupMocks();

            await CreateSut().Handle(
                new DeleteFileWorkOrderCommand { Image = "test.jpg" }, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteWOImageAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
