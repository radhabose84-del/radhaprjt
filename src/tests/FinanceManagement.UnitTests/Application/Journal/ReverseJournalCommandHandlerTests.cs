using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Commands.ReverseJournal;
using FinanceManagement.Domain.Entities;
using FinanceManagement.UnitTests.TestData;
using MediatR;

namespace FinanceManagement.UnitTests.Application.Journal
{
    public sealed class ReverseJournalCommandHandlerTests
    {
        private readonly Mock<IJournalCommandRepository> _command = new(MockBehavior.Strict);
        private readonly Mock<IJournalQueryRepository> _query = new(MockBehavior.Strict);
        private readonly Mock<IFinancialYearLookup> _fy = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _tz = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        private ReverseJournalCommandHandler CreateSut() =>
            new(_command.Object, _query.Object, _fy.Object, _ip.Object, _tz.Object, _mediator.Object);

        [Fact]
        public async Task Handle_ReversesPostedVoucher_PostsMirror_AndMarksOriginalReversed()
        {
            const int originalId = 7;
            var original = JournalBuilders.ValidDto(originalId, voucherNo: "JV/2026-27/0001");   // TotalDr=TotalCr=1000, 2 lines

            _query.Setup(r => r.GetByIdAsync(originalId)).ReturnsAsync(original);
            _query.Setup(r => r.GetOpenPeriodByDateAsync(original.CompanyId, It.IsAny<DateOnly>())).ReturnsAsync(((int, int)?)(4, 3));
            _query.Setup(r => r.GetStatusIdAsync("DRAFT")).ReturnsAsync(101);
            _query.Setup(r => r.GetStatusIdAsync("POSTED")).ReturnsAsync(105);
            _query.Setup(r => r.GetStatusIdAsync("REVERSED")).ReturnsAsync(106);
            _query.Setup(r => r.GetSourceIdAsync("MANUAL")).ReturnsAsync(111);
            _fy.Setup(f => f.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearLookupDto { FinancialYearId = 3, FinancialYearName = "2026-27" });
            _ip.Setup(s => s.GetUserId()).Returns(1);
            _tz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);

            JournalHeader? captured = null;
            _command.Setup(r => r.ReverseAsync(
                    It.IsAny<JournalHeader>(), originalId, 105, 106, "2026-27",
                    It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .Callback<JournalHeader, int, int, int, string?, string?, int, DateTimeOffset, CancellationToken>(
                    (h, _, _, _, _, _, _, _, _) => captured = h)
                .ReturnsAsync(new PostJournalResultDto { JournalId = 50, VoucherNo = "JV/2026-27/0002" });

            var result = await CreateSut().Handle(
                new ReverseJournalCommand { Id = originalId, ReversalDate = new DateOnly(2026, 6, 15) }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.VoucherNo.Should().Be("JV/2026-27/0002");

            // Mirror passed to ReverseAsync: IsReversal, linked, totals swapped.
            captured.Should().NotBeNull();
            captured!.IsReversal.Should().BeTrue();
            captured.ReversalOfId.Should().Be(originalId);
            captured.TotalDr.Should().Be(original.TotalCr);
            captured.TotalCr.Should().Be(original.TotalDr);
            captured.Narration.Should().Be("Reversal of JV/2026-27/0001");   // AC-2 prefix

            _command.Verify(r => r.ReverseAsync(
                It.IsAny<JournalHeader>(), originalId, 105, 106, "2026-27",
                It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NoDate_DefaultsToFirstDayOfNextOpenPeriod()
        {
            const int originalId = 7;
            var original = JournalBuilders.ValidDto(originalId, voucherNo: "JV/2026-27/0001");

            _query.Setup(r => r.GetByIdAsync(originalId)).ReturnsAsync(original);
            _query.Setup(r => r.GetNextOpenPeriodStartAsync(original.CompanyId, It.IsAny<DateOnly>()))
                .ReturnsAsync(new DateOnly(2026, 7, 1));
            _query.Setup(r => r.GetOpenPeriodByDateAsync(original.CompanyId, new DateOnly(2026, 7, 1))).ReturnsAsync(((int, int)?)(5, 3));
            _query.Setup(r => r.GetStatusIdAsync("DRAFT")).ReturnsAsync(101);
            _query.Setup(r => r.GetStatusIdAsync("POSTED")).ReturnsAsync(105);
            _query.Setup(r => r.GetStatusIdAsync("REVERSED")).ReturnsAsync(106);
            _query.Setup(r => r.GetSourceIdAsync("MANUAL")).ReturnsAsync(111);
            _fy.Setup(f => f.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearLookupDto { FinancialYearId = 3, FinancialYearName = "2026-27" });
            _ip.Setup(s => s.GetUserId()).Returns(1);
            _tz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);

            JournalHeader? captured = null;
            _command.Setup(r => r.ReverseAsync(
                    It.IsAny<JournalHeader>(), originalId, 105, 106, "2026-27",
                    It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .Callback<JournalHeader, int, int, int, string?, string?, int, DateTimeOffset, CancellationToken>(
                    (h, _, _, _, _, _, _, _, _) => captured = h)
                .ReturnsAsync(new PostJournalResultDto { JournalId = 50, VoucherNo = "JV/2026-27/0002" });

            var result = await CreateSut().Handle(
                new ReverseJournalCommand { Id = originalId, ReversalDate = null }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            captured!.VoucherDate.Should().Be(new DateOnly(2026, 7, 1));   // defaulted
            _query.Verify(r => r.GetNextOpenPeriodStartAsync(original.CompanyId, It.IsAny<DateOnly>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_Throws()
        {
            _query.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((JournalHeaderDto?)null);

            var act = async () => await CreateSut().Handle(
                new ReverseJournalCommand { Id = 99, ReversalDate = new DateOnly(2026, 6, 15) }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*not found*");
        }
    }
}
