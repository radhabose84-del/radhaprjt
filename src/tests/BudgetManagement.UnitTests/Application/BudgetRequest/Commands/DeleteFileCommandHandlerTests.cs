using BudgetManagement.Application.BudgetRequest.Commands.DeleteImage;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Extensions.Logging;

namespace BudgetManagement.UnitTests.Application.BudgetRequest.Commands
{
    public sealed class DeleteFileCommandHandlerTests
    {
        private readonly Mock<IBudgetRequestQueryRepository> _mockQuery = new(MockBehavior.Strict);
        private readonly Mock<IBudgetRequestCommandRepository> _mockCommand = new(MockBehavior.Strict);
        private readonly Mock<IFileUploadService> _mockFileService = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Strict);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DeleteFileCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private DeleteFileCommandHandler CreateSut() =>
            new(
                _mockLogger.Object,
                _mockQuery.Object,
                _mockFileService.Object,
                _mockCommand.Object,
                _mockIp.Object,
                _mockUnitLookup.Object,
                _mockCompanyLookup.Object);

        private void SetupDefaults(string baseDir = "uploads", bool deleteResult = true)
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIp.Setup(s => s.GetUnitId()).Returns(2);
            _mockCompanyLookup.Setup(s => s.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto>
                {
                    new() { CompanyId = 1, CompanyName = "Co" }
                });
            _mockUnitLookup.Setup(s => s.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new() { UnitId = 2, UnitName = "U1" }
                });
            _mockQuery.Setup(q => q.GetBaseDirectoryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(baseDir);
            _mockFileService.Setup(f => f.DeleteFileAsync(It.IsAny<string>())).ReturnsAsync(deleteResult);
            _mockCommand.Setup(c => c.RemoveImageReferenceAsync(It.IsAny<string>())).ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ShouldReturn_True_WhenFileServiceReturnsTrue()
        {
            SetupDefaults();
            var result = await CreateSut().Handle(
                new DeleteFileCommand { imagePath = "test.png" },
                CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenBaseDirectory_IsEmpty()
        {
            SetupDefaults(baseDir: "");

            Func<Task> act = async () => await CreateSut().Handle(
                new DeleteFileCommand { imagePath = "test.png" },
                CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Base directory not configured.");
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenFileServiceReturnsFalse()
        {
            SetupDefaults(deleteResult: false);

            Func<Task> act = async () => await CreateSut().Handle(
                new DeleteFileCommand { imagePath = "x.png" },
                CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("File deletion failed");
        }

        [Fact]
        public async Task Handle_Should_Call_RemoveImageReference_Once()
        {
            SetupDefaults();

            await CreateSut().Handle(
                new DeleteFileCommand { imagePath = "cleanup.png" },
                CancellationToken.None);

            _mockCommand.Verify(c => c.RemoveImageReferenceAsync("cleanup.png"), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Use_Empty_ImagePath_When_Null()
        {
            SetupDefaults();

            var result = await CreateSut().Handle(
                new DeleteFileCommand { imagePath = null },
                CancellationToken.None);

            result.Should().BeTrue();
            _mockCommand.Verify(c => c.RemoveImageReferenceAsync(string.Empty), Times.Once);
        }
    }
}
