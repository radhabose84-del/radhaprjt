using BackgroundService.Application.Workflow.ApprovalRequests.Queries.ApprovalDocumentDownload;
using BackgroundService.Application.Workflow.Common.Interfaces;
using Microsoft.Extensions.Hosting;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalRequest.Queries
{
    public sealed class DownloadFileQueryHandlerTests
    {
        private readonly Mock<IHostEnvironment> _mockEnv = new(MockBehavior.Loose);
        private readonly Mock<IFileStorageService> _mockFileStorage = new(MockBehavior.Strict);

        private DownloadFileQueryHandler CreateSut() =>
            new(_mockEnv.Object, _mockFileStorage.Object);

        [Fact]
        public void CanBeConstructed()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_FileNotFound_ThrowsFileNotFoundException()
        {
            _mockEnv.Setup(e => e.ContentRootPath).Returns("C:/nonexistent");

            var sut = CreateSut();
            var query = new DownloadFileQuery { RelativePath = "missing/file.pdf" };

            Func<Task> act = async () => await sut.Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<FileNotFoundException>();
        }
    }
}
