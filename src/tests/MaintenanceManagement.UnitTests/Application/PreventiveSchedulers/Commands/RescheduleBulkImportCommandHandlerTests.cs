using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IBackgroundService;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport;
using Microsoft.AspNetCore.Http;

namespace MaintenanceManagement.UnitTests.Application.PreventiveSchedulers.Commands.BatchD
{
    public sealed class RescheduleBulkImportCommandHandlerTests
    {
        private readonly Mock<IPreventiveSchedulerCommand> _mockCommand = new(MockBehavior.Loose);
        private readonly Mock<IBackgroundServiceClient> _mockBgClient = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IHttpContextAccessor> _mockHttp = new(MockBehavior.Loose);

        private RescheduleBulkImportCommandHandler CreateSut() =>
            new(_mockCommand.Object, _mockBgClient.Object, _mockQuery.Object,
                _mockMapper.Object, _mockHttp.Object);

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            Action act = () => { _ = CreateSut(); };
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Handle_EmptyFile_Throws()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            mockFile.Setup(f => f.Length).Returns(0);

            Func<Task> act = async () => await CreateSut().Handle(
                new RescheduleBulkImportCommand { File = mockFile.Object }, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
