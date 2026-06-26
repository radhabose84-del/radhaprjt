using System.Text;
using FinanceManagement.Application.ScheduleIII.Services;

namespace FinanceManagement.UnitTests.Application.ScheduleIII
{
    public sealed class ScheduleIIIImportFileServiceTests
    {
        private readonly ScheduleIIIImportFileService _sut = new();

        [Fact]
        public void BuildTemplate_Then_Parse_RoundTrips_SampleRows()
        {
            var template = _sut.BuildTemplate();

            using var stream = new MemoryStream(template.Content);
            var (rows, errors) = _sut.Parse(stream, template.FileName);

            errors.Should().BeEmpty();
            rows.Should().HaveCount(3);
            rows[0].SectionName.Should().Be("Revenue from Operations");
            rows[0].StatementType.Should().Be("PL");
            rows[0].Nature.Should().Be("Income");
            rows[0].LineCode.Should().Be("REV01");
            rows[0].LineName.Should().Be("Sale of products");
            rows[0].IsSplitLine.Should().BeFalse();
            rows[2].SectionName.Should().Be("Other Expenses");
            rows[2].LineCode.Should().Be("EXP01");
        }

        [Fact]
        public void Parse_Csv_ReadsRows()
        {
            var csv = "SectionName,StatementType,Nature,LineCode,LineName,NoteReference,IsSplitLine\n" +
                      "Revenue,PL,Income,REV01,Sale of products,Note 20,false\n" +
                      "Revenue,PL,Income,REV02,Other income,Note 21,true\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            var (rows, errors) = _sut.Parse(stream, "import.csv");

            errors.Should().BeEmpty();
            rows.Should().HaveCount(2);
            rows[1].LineCode.Should().Be("REV02");
            rows[1].IsSplitLine.Should().BeTrue();
        }

        [Fact]
        public void Parse_MissingRequiredColumn_ReturnsFileError()
        {
            // No LineName column.
            var csv = "SectionName,StatementType,Nature,LineCode\nRevenue,PL,Income,REV01\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            var (rows, errors) = _sut.Parse(stream, "bad.csv");

            rows.Should().BeEmpty();
            errors.Should().ContainSingle().Which.Message.Should().Contain("LineName");
        }
    }
}
