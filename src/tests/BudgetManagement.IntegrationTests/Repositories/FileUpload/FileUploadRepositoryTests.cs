using System.Text;
using BudgetManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BudgetManagement.IntegrationTests.Repositories.FileUpload
{
    public sealed class FileUploadRepositoryTests : IDisposable
    {
        private readonly string _tempRoot;

        public FileUploadRepositoryTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "bsoft-bm-fs-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempRoot))
                Directory.Delete(_tempRoot, recursive: true);
        }

        private FileUploadRepository CreateRepo()
        {
            var httpMock = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
            return new FileUploadRepository(httpMock.Object);
        }

        private static IFormFile BuildFile(string fileName, string content = "hello")
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };
        }

        [Fact]
        public async Task UploadFileAsync_Should_Return_Success_And_FilePath()
        {
            var file = BuildFile("pic.png", "content");

            var result = await CreateRepo().UploadFileAsync(file, _tempRoot);

            result.IsSuccess.Should().BeTrue();
            result.FilePath.Should().NotBeNullOrEmpty();
            File.Exists(result.FilePath).Should().BeTrue();
        }

        [Fact]
        public async Task UploadFileAsync_Should_Persist_Content()
        {
            var file = BuildFile("c.txt", "persisted");

            var result = await CreateRepo().UploadFileAsync(file, _tempRoot);

            (await File.ReadAllTextAsync(result.FilePath)).Should().Be("persisted");
        }

        [Fact]
        public async Task UploadFileAsync_Should_Return_Base64_Of_File()
        {
            var file = BuildFile("b.txt", "abc");

            var result = await CreateRepo().UploadFileAsync(file, _tempRoot);

            result.logoBase64.Should().NotBeNullOrEmpty();
            Convert.FromBase64String(result.logoBase64).Should().Equal(Encoding.UTF8.GetBytes("abc"));
        }

        [Fact]
        public async Task UploadFileAsync_Should_Preserve_FileExtension()
        {
            var file = BuildFile("image.png", "fake");

            var result = await CreateRepo().UploadFileAsync(file, _tempRoot);

            Path.GetExtension(result.FilePath).Should().Be(".png");
        }

        [Fact]
        public async Task DeleteFileAsync_Should_Remove_File_And_Return_True()
        {
            var path = Path.Combine(_tempRoot, "del.txt");
            await File.WriteAllTextAsync(path, "to-delete");

            var result = await CreateRepo().DeleteFileAsync(path);

            result.Should().BeTrue();
            File.Exists(path).Should().BeFalse();
        }
    }
}
