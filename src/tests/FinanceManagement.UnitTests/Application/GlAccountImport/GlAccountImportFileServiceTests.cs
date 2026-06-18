using System.IO;
using System.Linq;
using FinanceManagement.Application.GlAccountImport.Dto;
using FinanceManagement.Application.GlAccountImport.Services;

namespace FinanceManagement.UnitTests.Application.GlAccountImport
{
    /// <summary>
    /// File reader/writer tests — the round-trip ones back the "export re-imports cleanly" goal (AC5).
    /// </summary>
    public sealed class GlAccountImportFileServiceTests
    {
        private static readonly GlAccountImportFileService Service = new();

        private static (System.Collections.Generic.IReadOnlyList<GlAccountImportRowDto> Rows,
            System.Collections.Generic.IReadOnlyList<GlAccountImportErrorDto> Errors)
            ParseBytes(byte[] bytes, string format)
        {
            using var stream = new MemoryStream(bytes);
            return Service.Parse(stream, format);
        }

        [Theory]
        [InlineData(GlAccountImportFileService.FormatExcel)]
        [InlineData(GlAccountImportFileService.FormatCsv)]
        public void Template_parses_to_the_two_sample_rows(string format)
        {
            var template = Service.BuildTemplate(format);

            var (rows, errors) = ParseBytes(template.Content, format);

            errors.Should().BeEmpty();
            rows.Should().HaveCount(2);
            rows.Should().Contain(r => r.RecordType == "GROUP");
            rows.Should().Contain(r => r.RecordType == "ACCOUNT");
        }

        [Theory]
        [InlineData(GlAccountImportFileService.FormatExcel)]
        [InlineData(GlAccountImportFileService.FormatCsv)]
        public void Export_round_trips_through_the_parser(string format)
        {
            var source = new[]
            {
                new GlAccountImportRowDto
                {
                    RecordType = "GROUP", GroupCode = "1000", GroupName = "Cash, Bank",  // comma → CSV quoting
                    ParentGroupCode = null, AccountType = "Asset", SortOrder = "1"
                },
                new GlAccountImportRowDto
                {
                    RecordType = "ACCOUNT", AccountCode = "100001", AccountName = "Petty \"cash\"", // quote → escaping
                    Description = "Line\nbreak", AccountGroupCode = "1000",
                    NormalBalance = "DR", Currency = "INRONLY", SubLedgerType = "NONE",
                    IsCostCentreMandatory = "1", IsTaxRelevant = "0", IsInterCompany = "0", IsReconciliationRequired = "0"
                }
            };

            var file = Service.BuildExport(source, format);
            var (rows, errors) = ParseBytes(file.Content, format);

            errors.Should().BeEmpty();
            rows.Should().HaveCount(2);

            var group = rows.Single(r => r.RecordType == "GROUP");
            group.GroupCode.Should().Be("1000");
            group.GroupName.Should().Be("Cash, Bank");
            group.AccountType.Should().Be("Asset");

            var account = rows.Single(r => r.RecordType == "ACCOUNT");
            account.AccountCode.Should().Be("100001");
            account.AccountName.Should().Be("Petty \"cash\"");
            account.AccountGroupCode.Should().Be("1000");
            account.IsCostCentreMandatory.Should().Be("1");
        }

        [Fact]
        public void Csv_missing_record_type_column_yields_a_file_level_error()
        {
            var csv = "GroupCode,GroupName\n1000,Assets\n";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);

            var (rows, errors) = ParseBytes(bytes, GlAccountImportFileService.FormatCsv);

            rows.Should().BeEmpty();
            errors.Should().Contain(e => e.ErrorMessage.Contains("RecordType"));
        }

        [Fact]
        public void Csv_columns_can_be_reordered_and_still_map_by_header()
        {
            var csv = "GroupName,RecordType,AccountType,GroupCode\nAssets,GROUP,Asset,1000\n";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);

            var (rows, errors) = ParseBytes(bytes, GlAccountImportFileService.FormatCsv);

            errors.Should().BeEmpty();
            var row = rows.Should().ContainSingle().Subject;
            row.RecordType.Should().Be("GROUP");
            row.GroupCode.Should().Be("1000");
            row.GroupName.Should().Be("Assets");
            row.AccountType.Should().Be("Asset");
        }

        [Theory]
        [InlineData("books.xlsx", GlAccountImportFileService.FormatExcel)]
        [InlineData("books.csv", GlAccountImportFileService.FormatCsv)]
        [InlineData("BOOKS.CSV", GlAccountImportFileService.FormatCsv)]
        public void Resolve_format_keys_off_the_extension(string fileName, string expected)
        {
            Service.IsSupported(fileName).Should().BeTrue();
            Service.ResolveFormat(fileName).Should().Be(expected);
        }

        [Fact]
        public void Unsupported_extension_is_rejected()
        {
            Service.IsSupported("data.txt").Should().BeFalse();
        }
    }
}
