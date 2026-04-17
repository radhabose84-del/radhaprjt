using System.Text;
using GateEntryManagement.Infrastructure.Services;
using Microsoft.Extensions.Hosting;

namespace GateEntryManagement.IntegrationTests.Repositories.FileUpload
{
    public sealed class FileUploadRepositoryTests : IDisposable
    {
        private readonly string _tempRoot;

        public FileUploadRepositoryTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "bsoft-ge-fs-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempRoot))
                Directory.Delete(_tempRoot, recursive: true);
        }

        private FileUploadRepository CreateRepo()
        {
            var hostMock = new Mock<IHostEnvironment>(MockBehavior.Loose);
            hostMock.Setup(h => h.ContentRootPath).Returns(_tempRoot);
            return new FileUploadRepository(hostMock.Object);
        }

        // --- UploadFileAsync ---

        [Fact]
        public async Task UploadFileAsync_Should_Return_Unique_FileName()
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("content"));

            var result = await CreateRepo().UploadFileAsync(base64, "pic.png", "uploads");

            result.Should().EndWith("_pic.png");
            result.Should().NotBe("pic.png");
        }

        [Fact]
        public async Task UploadFileAsync_Should_Create_Target_Folder_And_File()
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("hello"));

            var fileName = await CreateRepo().UploadFileAsync(base64, "file.txt", "sub/uploads");

            var savedPath = Path.Combine(_tempRoot, "sub/uploads", fileName);
            File.Exists(savedPath).Should().BeTrue();
            (await File.ReadAllTextAsync(savedPath)).Should().Be("hello");
        }

        [Fact]
        public async Task UploadFileAsync_Should_Decode_Base64_Correctly()
        {
            var data = new byte[] { 0x10, 0x20, 0x30, 0x40 };
            var base64 = Convert.ToBase64String(data);

            var fileName = await CreateRepo().UploadFileAsync(base64, "b.bin", "files");

            var saved = Path.Combine(_tempRoot, "files", fileName);
            (await File.ReadAllBytesAsync(saved)).Should().Equal(data);
        }

        // --- DeleteFileAsync ---

        [Fact]
        public async Task DeleteFileAsync_Should_Return_True_When_File_Exists()
        {
            var path = Path.Combine(_tempRoot, "to-delete.txt");
            await File.WriteAllTextAsync(path, "x");

            var result = await CreateRepo().DeleteFileAsync(path);

            result.Should().BeTrue();
            File.Exists(path).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteFileAsync_Should_Return_False_When_File_Missing()
        {
            var path = Path.Combine(_tempRoot, "nothing.txt");

            var result = await CreateRepo().DeleteFileAsync(path);

            result.Should().BeFalse();
        }
    }
}
