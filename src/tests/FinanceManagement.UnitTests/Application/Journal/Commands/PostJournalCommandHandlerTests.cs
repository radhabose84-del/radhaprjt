using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournal;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.Journal.Commands
{
    public sealed class PostJournalCommandHandlerTests
    {
        private readonly Mock<IJournalCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IJournalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private PostJournalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockTz.Object, _mockMediator.Object);

        private void SetupHappyPath(string voucherNo = "JV/2026-27/0001")
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(JournalBuilders.ValidDto());
            _mockQueryRepo.Setup(r => r.GetStatusIdAsync("POSTED")).ReturnsAsync(105);
            _mockIp.Setup(s => s.GetUserId()).Returns(1);
            _mockTz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
            _mockCommandRepo
                .Setup(r => r.PostAsync(1, 105, It.IsAny<string?>(), 1, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JournalBuilders.ValidPostResult(voucherNo));
        }

        [Fact]
        public async Task Handle_ValidPost_ReturnsVoucherNo()
        {
            SetupHappyPath("JV/2026-27/0042");
            var result = await CreateSut().Handle(new PostJournalCommand(1), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.VoucherNo.Should().Be("JV/2026-27/0042");
        }

        [Fact]
        public async Task Handle_ValidPost_ReturnsUpdatedBalances()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new PostJournalCommand(1), CancellationToken.None);

            result.Data!.UpdatedBalances.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_NotFound_Throws()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((JournalHeaderDto?)null);

            var act = async () => await CreateSut().Handle(new PostJournalCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_RepoReturnsNull_Throws()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(JournalBuilders.ValidDto());
            _mockQueryRepo.Setup(r => r.GetStatusIdAsync("POSTED")).ReturnsAsync(105);
            _mockIp.Setup(s => s.GetUserId()).Returns(1);
            _mockTz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
            _mockCommandRepo
                .Setup(r => r.PostAsync(1, 105, It.IsAny<string?>(), 1, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PostJournalResultDto?)null);

            var act = async () => await CreateSut().Handle(new PostJournalCommand(1), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidPost_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new PostJournalCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Post" && e.ActionCode == "JOURNAL_POST"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
