using System.Text;
using FinanceManagement.Application.JournalMaster.JournalImport.Services;

namespace FinanceManagement.UnitTests.Application.JournalImport
{
    public sealed class JournalImportFileServiceTests
    {
        private readonly JournalImportFileService _svc = new();

        [Fact]
        public void BuildTemplate_RoundTrips_ToTwoSampleRows()
        {
            var tmpl = _svc.BuildTemplate();
            tmpl.FileName.Should().EndWith(".xlsx");

            using var ms = new MemoryStream(tmpl.Content);
            var (rows, errors) = _svc.Parse(ms, tmpl.FileName);

            errors.Should().BeEmpty();
            rows.Should().HaveCount(2);
            rows[0].GroupNo.Should().Be(1);
            rows[0].VoucherDate.Should().Be(new DateOnly(2026, 6, 10));
            rows[0].DrAmount.Should().Be(100000m);
            rows[1].CrAmount.Should().Be(100000m);
        }

        [Fact]
        public void Csv_Parses_BalancedGroup()
        {
            const string csv =
                "GroupNo,VoucherTypeId,VoucherDate,GlAccountId,CurrencyId,DrAmount,CrAmount\n" +
                "1,1,2026-06-10,5,1,1000,0\n" +
                "1,1,2026-06-10,6,1,0,1000\n";
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            var (rows, errors) = _svc.Parse(ms, "x.csv");

            errors.Should().BeEmpty();
            rows.Should().HaveCount(2);
            rows[0].GlAccountId.Should().Be(5);
            rows[1].CrAmount.Should().Be(1000m);
        }

        [Fact]
        public void Csv_BadDate_ProducesRowParseError()
        {
            const string csv =
                "GroupNo,VoucherTypeId,VoucherDate,GlAccountId,CurrencyId,DrAmount,CrAmount\n" +
                "1,1,notadate,5,1,1000,0\n";
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            var (_, errors) = _svc.Parse(ms, "x.csv");

            errors.Should().Contain(e => e.ColumnName == "VoucherDate");
        }

        [Fact]
        public void Csv_MissingRequiredColumn_ProducesFileError()
        {
            const string csv = "GroupNo,VoucherTypeId\n1,1\n";
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            var (_, errors) = _svc.Parse(ms, "x.csv");

            errors.Should().Contain(e => e.Message!.Contains("Missing required"));
        }
    }
}
