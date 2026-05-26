using QCManagement.Application.AuditLog.Queries.GetAuditLog;
using QCManagement.Application.AuditLog.Queries.GetAuditLogAutoComplete;
using QCManagement.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace QCManagement.UnitTests.Controllers
{
    public sealed class AuditLogControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private AuditLogController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AuditLogDto>());

            var result = await CreateSut().GetAllAuditLogsAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AuditLogDto>());

            await CreateSut().GetAllAuditLogsAsync();

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAuditLogAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = true,
                    Data = new List<AuditLogDto>()
                });

            var result = await CreateSut().GetAuditLogAutoCompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAutoComplete_CallsMediatorOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAuditLogAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = false,
                    Message = "No audit logs found matching the search pattern."
                });

            await CreateSut().GetAuditLogAutoCompleteAsync("xyz");

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAuditLogAutoCompleteQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
