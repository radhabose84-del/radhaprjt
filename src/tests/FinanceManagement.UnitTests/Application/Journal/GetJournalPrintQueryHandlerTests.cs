using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetJournalPrint;
using FinanceManagement.UnitTests.TestData;
using MediatR;

namespace FinanceManagement.UnitTests.Application.Journal
{
    public sealed class GetJournalPrintQueryHandlerTests
    {
        private readonly Mock<IJournalQueryRepository> _query = new(MockBehavior.Strict);
        private readonly Mock<ICompanyLookup> _company = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        private GetJournalPrintQueryHandler CreateSut() => new(_query.Object, _company.Object, _mediator.Object);

        private void SetupCompany() =>
            _company.Setup(c => c.GetAllCompanyAsync()).ReturnsAsync(new List<CompanyLookupDto>
            {
                new() { CompanyId = 1, CompanyName = "BM", LegalName = "Bannari Mills Ltd", GstNumber = "26AABCB1234C1Z5" }
            });

        [Fact]
        public async Task Handle_BuildsPrintModel_WithHeaderLinesAndCompany()
        {
            _query.Setup(r => r.GetByIdAsync(45)).ReturnsAsync(JournalBuilders.ValidDto(45, voucherNo: "JV/2026-27/0428"));
            SetupCompany();

            var result = await CreateSut().Handle(new GetJournalPrintQuery(45), CancellationToken.None);

            result.Should().NotBeNull();
            result!.VoucherNo.Should().Be("JV/2026-27/0428");
            result.CompanyLegalName.Should().Be("Bannari Mills Ltd");
            result.CompanyGstin.Should().Be("26AABCB1234C1Z5");
            result.Lines.Should().HaveCount(2);
            result.TotalDr.Should().Be(1000m);
            result.VerifyUrl.Should().Be("/fin/jv/JV/2026-27/0428");
            result.Fingerprint.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Handle_Fingerprint_IsDeterministic()
        {
            _query.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(JournalBuilders.ValidDto(45, voucherNo: "JV/2026-27/0428"));
            SetupCompany();

            var a = await CreateSut().Handle(new GetJournalPrintQuery(45), CancellationToken.None);
            var b = await CreateSut().Handle(new GetJournalPrintQuery(45), CancellationToken.None);

            a!.Fingerprint.Should().Be(b!.Fingerprint);
            a.Fingerprint.Should().HaveLength(16);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _query.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((JournalHeaderDto?)null);

            var result = await CreateSut().Handle(new GetJournalPrintQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
