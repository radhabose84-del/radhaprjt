using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.ImportScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Entities;
using MediatR;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class ImportScheduleIIICommandHandlerTests
    {
        private readonly Mock<IScheduleIIIImportCommandRepository> _command = new(MockBehavior.Strict);
        private readonly Mock<IScheduleIIIImportQueryRepository> _query = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        private ImportScheduleIIICommandHandler CreateSut() => new(_command.Object, _query.Object, _mediator.Object);

        private void SetupLookups(params string[] existingSections)
        {
            _query.Setup(r => r.GetStatementTypeOptionsAsync()).ReturnsAsync(new List<ScheduleIIIMiscOptionDto>
            {
                new() { Id = 1, Code = "PL", Description = "Statement of P&L" },
                new() { Id = 2, Code = "BS", Description = "Balance Sheet" }
            });
            _query.Setup(r => r.GetNatureOptionsAsync()).ReturnsAsync(new List<ScheduleIIIMiscOptionDto>
            {
                new() { Id = 10, Code = "Income", Description = "Income" },
                new() { Id = 11, Code = "Expense", Description = "Expense" }
            });
            _query.Setup(r => r.GetExistingSectionNamesAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(existingSections.ToList());
            _query.Setup(r => r.GetMaxSectionDisplayOrderAsync()).ReturnsAsync(0);
        }

        private static List<ScheduleIIIImportRowInputDto> TwoLineSection() => new()
        {
            new() { RowNo = 2, SectionName = "Revenue", StatementType = "PL", Nature = "Income", LineCode = "REV01", LineName = "Sale of products" },
            new() { RowNo = 3, SectionName = "Revenue", StatementType = "PL", Nature = "Income", LineCode = "REV02", LineName = "Other income" }
        };

        [Fact]
        public async Task Handle_Valid_CommitsSectionWithItems()
        {
            SetupLookups();
            _command.Setup(r => r.CommitAsync(
                    It.IsAny<IReadOnlyList<(ScheduleIIISection, List<ScheduleIIISectionItem>)>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((1, 2, new List<int> { 55 }));

            var result = await CreateSut().Handle(
                new ImportScheduleIIICommand { FileName = "s3.xlsx", Rows = TwoLineSection() }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.Committed.Should().BeTrue();
            result.Data.Status.Should().Be("COMMITTED");
            result.Data.SectionsCreated.Should().Be(1);
            result.Data.ItemsCreated.Should().Be(2);
            result.Data.CreatedSectionIds.Should().BeEquivalentTo(new[] { 55 });
            _command.Verify(r => r.CommitAsync(It.IsAny<IReadOnlyList<(ScheduleIIISection, List<ScheduleIIISectionItem>)>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Valid_SequencesSectionDisplayOrderAfterExistingMax()
        {
            SetupLookups();
            _query.Setup(r => r.GetMaxSectionDisplayOrderAsync()).ReturnsAsync(5);   // catalog already has up to 5

            IReadOnlyList<(ScheduleIIISection Section, List<ScheduleIIISectionItem> Items)>? captured = null;
            _command.Setup(r => r.CommitAsync(
                    It.IsAny<IReadOnlyList<(ScheduleIIISection, List<ScheduleIIISectionItem>)>>(), It.IsAny<CancellationToken>()))
                .Callback<IReadOnlyList<(ScheduleIIISection, List<ScheduleIIISectionItem>)>, CancellationToken>((g, _) => captured = g)
                .ReturnsAsync((2, 2, new List<int> { 6, 7 }));

            var rows = new List<ScheduleIIIImportRowInputDto>
            {
                new() { RowNo = 2, SectionName = "Revenue",  StatementType = "PL", Nature = "Income",  LineCode = "REV01", LineName = "Sale" },
                new() { RowNo = 3, SectionName = "Expenses", StatementType = "PL", Nature = "Expense", LineCode = "EXP01", LineName = "Power" }
            };

            var result = await CreateSut().Handle(
                new ImportScheduleIIICommand { FileName = "s3.xlsx", Rows = rows }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            captured.Should().NotBeNull();
            captured![0].Section.DisplayOrder.Should().Be(6);   // 5 + 1
            captured[1].Section.DisplayOrder.Should().Be(7);    // 5 + 2
        }

        [Fact]
        public async Task Handle_SectionAlreadyExists_Fails_NoCommit()
        {
            SetupLookups(existingSections: "Revenue");

            var result = await CreateSut().Handle(
                new ImportScheduleIIICommand { FileName = "s3.xlsx", Rows = TwoLineSection() }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data!.Committed.Should().BeFalse();
            result.Data.Status.Should().Be("FAILED");
            result.Data.Errors.Should().Contain(e => e.Message!.Contains("already exists"));
            _command.Verify(r => r.CommitAsync(It.IsAny<IReadOnlyList<(ScheduleIIISection, List<ScheduleIIISectionItem>)>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UnknownStatementType_Fails()
        {
            SetupLookups();
            var rows = TwoLineSection();
            rows[0].StatementType = "ZZ";
            rows[1].StatementType = "ZZ";

            var result = await CreateSut().Handle(
                new ImportScheduleIIICommand { FileName = "s3.xlsx", Rows = rows }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data!.Errors.Should().Contain(e => e.ColumnName == "StatementType");
            _command.Verify(r => r.CommitAsync(It.IsAny<IReadOnlyList<(ScheduleIIISection, List<ScheduleIIISectionItem>)>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DuplicateLineCodeInSection_Fails()
        {
            SetupLookups();
            var rows = TwoLineSection();
            rows[1].LineCode = "REV01";   // duplicate within the section

            var result = await CreateSut().Handle(
                new ImportScheduleIIICommand { FileName = "s3.xlsx", Rows = rows }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data!.Errors.Should().Contain(e => e.Message!.Contains("Duplicate line code"));
        }
    }
}
