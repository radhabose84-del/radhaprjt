using FinanceManagement.Application.Common.Interfaces.ICoaReport;
using FinanceManagement.Application.CoaReport.Dto;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FinanceManagement.Infrastructure.Repositories.CoaReport
{
    // US-GL02-15 (AC1/AC5) — auditor-ready COA listing PDF via QuestPDF. Renders from already-fetched
    // rows (no DB work here), so generation stays inside the AC1 10-second budget.
    public class CoaListingPdfBuilder : ICoaListingPdfBuilder
    {
        public byte[] Build(string? companyName, DateTimeOffset generatedOn, IReadOnlyList<CoaListingItemDto> rows)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(18);
                    page.DefaultTextStyle(t => t.FontSize(8));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Chart of Accounts — Listing & Structure").Bold().FontSize(14);
                        col.Item().Text($"Company: {companyName ?? "-"}").FontSize(9);
                        col.Item().Text($"Generated: {generatedOn:yyyy-MM-dd HH:mm} | Accounts: {rows.Count}").FontSize(8)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingVertical(6).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60);   // code
                            columns.RelativeColumn(3);     // account name (indented by level)
                            columns.RelativeColumn(2);     // group
                            columns.RelativeColumn(2);     // type
                            columns.ConstantColumn(34);    // active
                            columns.ConstantColumn(40);    // postings
                            columns.RelativeColumn(2);     // FS line
                            columns.ConstantColumn(34);    // attrs
                        });

                        table.Header(header =>
                        {
                            void H(string t) => header.Cell().Element(HeaderCell).Text(t).Bold();
                            H("Code"); H("Account"); H("Group"); H("Type");
                            H("Active"); H("Postings"); H("FS Line"); H("Flags");
                        });

                        foreach (var r in rows)
                        {
                            var indent = Math.Max(0, r.GroupLevel) * 6;
                            table.Cell().Element(Cell).Text(r.AccountCode ?? "");
                            table.Cell().Element(Cell).PaddingLeft(indent).Text(r.AccountName ?? "");
                            table.Cell().Element(Cell).Text(r.GroupName ?? "");
                            table.Cell().Element(Cell).Text(r.AccountTypeName ?? "");
                            table.Cell().Element(Cell).Text(r.IsActive ? "Yes" : "No");
                            table.Cell().Element(Cell).AlignRight().Text(r.PostingCount.ToString());
                            table.Cell().Element(Cell).Text(FsLine(r));
                            table.Cell().Element(Cell).Text(Flags(r));
                        }
                    });

                    page.Footer().AlignRight().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();

            static IContainer HeaderCell(IContainer c) =>
                c.Background(Colors.Grey.Lighten2).BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(3);

            static IContainer Cell(IContainer c) =>
                c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).PaddingHorizontal(3);
        }

        private static string FsLine(CoaListingItemDto r) =>
            r.ScheduleIIISectionItemId == null
                ? "UNMAPPED"
                : $"{r.StatementTypeCode}: {r.FsLineCode} {r.FsLineName}".Trim();

        private static string Flags(CoaListingItemDto r)
        {
            var flags = new List<string>();
            if (r.IsCostCentreMandatory) flags.Add("CC");
            if (r.IsProfitCentreMandatory) flags.Add("PC");
            if (r.IsTaxRelevant) flags.Add("TX");
            if (r.IsInterCompany) flags.Add("IC");
            if (r.IsReconciliationRequired) flags.Add("RC");
            return string.Join(" ", flags);
        }
    }
}
