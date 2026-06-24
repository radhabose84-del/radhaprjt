using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.AccountAuditTrail.Dto;
using FinanceManagement.Application.AccountAuditTrail.Queries.ExportAccountAudit;
using FinanceManagement.Application.AccountAuditTrail.Queries.GetAccountAuditHistory;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class AccountAuditTrailControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private AccountAuditTrailController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetHistory_ReturnsOk_AndCallsMediatorOnce()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAccountAuditHistoryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AccountAuditTrailDto>());

            (await CreateSut().GetHistoryAsync("GlAccountMaster", 42)).Should().BeOfType<OkObjectResult>();
            _mockMediator.Verify(m => m.Send(It.IsAny<GetAccountAuditHistoryQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Export_ReturnsOk_AndCallsMediatorOnce()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<ExportAccountAuditQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccountAuditExportDto { RecordCount = 0, Checksum = "ABC" });

            var from = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var to = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);
            (await CreateSut().ExportAsync(from, to, "GlAccountMaster")).Should().BeOfType<OkObjectResult>();
            _mockMediator.Verify(m => m.Send(It.IsAny<ExportAccountAuditQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
