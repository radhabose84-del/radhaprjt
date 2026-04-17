using System.ComponentModel.DataAnnotations;
using System.Text;
using BudgetManagement.Application.BudgetRequest.Commands;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Application.Quotation.QuotationEntry.Commands.UploadItemImage;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BudgetManagement.UnitTests.Application.BudgetRequest.Commands
{
    public sealed class UploadFileCommandHandlerTests : IDisposable
    {
        private readonly Mock<IBudgetRequestQueryRepository> _mockQuery = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Strict);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Strict);
        private readonly Mock<ILogger<UploadFileCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private readonly string _tempRoot;

        public UploadFileCommandHandlerTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "bsoft-bm-upload-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);
            Directory.SetCurrentDirectory(_tempRoot);
        }

        public void Dispose()
        {
            try { if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, recursive: true); }
            catch { /* ignore cleanup races */ }
        }

        private UploadFileCommandHandler CreateSut() =>
            new(
                _mockLogger.Object,
                _mockQuery.Object,
                _mockIp.Object,
                _mockUnitLookup.Object,
                _mockCompanyLookup.Object);

        private void SetupDefaults(string baseDir = "uploads")
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIp.Setup(s => s.GetUnitId()).Returns(2);
            _mockCompanyLookup.Setup(s => s.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Co" } });
            _mockUnitLookup.Setup(s => s.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto> { new() { UnitId = 2, UnitName = "U1" } });
            _mockQuery.Setup(q => q.GetBaseDirectoryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(baseDir);
        }

        private static IFormFile BuildFile(string fileName, string content = "hello")
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
        }

        [Fact]
        public async Task Handle_ShouldReturn_ImageDto_With_Image_And_Base64()
        {
            SetupDefaults();
            var command = new UploadFileCommand { File = BuildFile("pic.png", "content") };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Image.Should().NotBeNullOrEmpty();
            result.Image.Should().EndWith(".png");
            result.ImageBase64.Should().NotBeNullOrEmpty();
            Convert.FromBase64String(result.ImageBase64!).Should().Equal(Encoding.UTF8.GetBytes("content"));
        }

        [Fact]
        public async Task Handle_ShouldThrowValidation_WhenFile_IsNull()
        {
            var command = new UploadFileCommand { File = null };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("No file uploaded");
        }

        [Fact]
        public async Task Handle_ShouldThrowValidation_WhenFile_IsEmpty()
        {
            var command = new UploadFileCommand { File = BuildFile("empty.png", "") };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("No file uploaded");
        }

        [Fact]
        public async Task Handle_ShouldThrowValidation_WhenBaseDirectory_Empty()
        {
            SetupDefaults(baseDir: "");
            var command = new UploadFileCommand { File = BuildFile("ok.png", "x") };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Base directory not configured.");
        }

        [Fact]
        public async Task Handle_Should_Persist_File_Under_Expected_Path()
        {
            SetupDefaults();
            var command = new UploadFileCommand { File = BuildFile("persist.png", "persisted") };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            var expectedDir = Path.Combine(_tempRoot, "Resources", "uploads", "Co", "U1");
            Directory.Exists(expectedDir).Should().BeTrue();
            var saved = Path.Combine(expectedDir, result.Image!);
            File.Exists(saved).Should().BeTrue();
        }
    }
}
