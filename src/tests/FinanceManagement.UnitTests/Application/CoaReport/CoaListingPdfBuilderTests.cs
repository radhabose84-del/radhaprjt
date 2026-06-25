using FinanceManagement.Application.CoaReport.Dto;
using FinanceManagement.Infrastructure.Repositories.CoaReport;

namespace FinanceManagement.UnitTests.Application.CoaReport
{
    // US-GL02-15 (AC1/AC5) — the QuestPDF builder produces a valid, non-trivial PDF.
    public sealed class CoaListingPdfBuilderTests
    {
        public CoaListingPdfBuilderTests() =>
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        private static List<CoaListingItemDto> SampleRows() => new()
        {
            new() { AccountCode = "1000", AccountName = "Assets", GroupLevel = 1, IsActive = true, AccountTypeName = "Asset",
                    GroupName = "Assets", ScheduleIIISectionItemId = 7, StatementTypeCode = "BS", FsLineCode = "A1", FsLineName = "Cash", PostingCount = 3 },
            new() { AccountCode = "1001", AccountName = "Cash in Hand", GroupLevel = 3, IsActive = true, AccountTypeName = "Asset",
                    GroupName = "Current Assets", IsCostCentreMandatory = true, PostingCount = 0 },
        };

        [Fact]
        public void Build_ProducesValidPdfBytes()
        {
            var bytes = new CoaListingPdfBuilder().Build("Acme Mills", DateTimeOffset.Now, SampleRows());

            bytes.Should().NotBeNullOrEmpty();
            // PDF magic header "%PDF"
            System.Text.Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
            bytes.Length.Should().BeGreaterThan(500);
        }

        [Fact]
        public void Build_EmptyCoa_StillProducesPdf()
        {
            var bytes = new CoaListingPdfBuilder().Build("Acme Mills", DateTimeOffset.Now, new List<CoaListingItemDto>());

            bytes.Should().NotBeNullOrEmpty();
            System.Text.Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
        }
    }
}
