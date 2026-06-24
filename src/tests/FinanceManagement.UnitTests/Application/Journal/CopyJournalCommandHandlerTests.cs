using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Commands.CopyJournal;
using FinanceManagement.Domain.Entities;
using FinanceManagement.UnitTests.TestData;
using MediatR;

namespace FinanceManagement.UnitTests.Application.Journal
{
    public sealed class CopyJournalCommandHandlerTests
    {
        private readonly Mock<IJournalCommandRepository> _command = new(MockBehavior.Strict);
        private readonly Mock<IJournalQueryRepository> _query = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _tz = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        private CopyJournalCommandHandler CreateSut() =>
            new(_command.Object, _query.Object, _ip.Object, _tz.Object, _mediator.Object);

        [Fact]
        public async Task Handle_CopiesSourceIntoNewDraft()
        {
            const int sourceId = 7;
            var source = JournalBuilders.ValidDto(sourceId, voucherNo: "JV/2026-27/0001");   // 2 lines, TotalDr=TotalCr=1000

            _query.Setup(r => r.GetByIdAsync(sourceId)).ReturnsAsync(source);
            _query.Setup(r => r.GetOpenPeriodByDateAsync(source.CompanyId, It.IsAny<DateOnly>())).ReturnsAsync(((int, int)?)(4, 3));
            _query.Setup(r => r.GetStatusIdAsync("DRAFT")).ReturnsAsync(101);
            _query.Setup(r => r.GetSourceIdAsync("MANUAL")).ReturnsAsync(111);
            _ip.Setup(s => s.GetCompanyId()).Returns(1);
            _ip.Setup(s => s.GetUnitId()).Returns(1);
            _tz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(new DateTimeOffset(2026, 6, 16, 0, 0, 0, TimeSpan.Zero));

            JournalHeader? captured = null;
            _command.Setup(r => r.CreateAsync(It.IsAny<JournalHeader>()))
                .Callback<JournalHeader>(h => captured = h)
                .ReturnsAsync(60);

            var result = await CreateSut().Handle(new CopyJournalCommand { Id = sourceId }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(60);

            captured.Should().NotBeNull();
            captured!.VoucherNo.Should().BeNull();                              // blank — assigned on post
            captured.StatusId.Should().Be(101);                                // Draft
            captured.CopiedFromRef.Should().Be("JV/2026-27/0001");             // informational source ref
            captured.IsReversal.Should().BeFalse();
            captured.VoucherDate.Should().Be(new DateOnly(2026, 6, 16));       // today
            captured.Details.Should().HaveCount(source.Lines.Count);
        }

        [Fact]
        public async Task Handle_NotFound_Throws()
        {
            _query.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((JournalHeaderDto?)null);

            var act = async () => await CreateSut().Handle(new CopyJournalCommand { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*not found*");
        }
    }
}
