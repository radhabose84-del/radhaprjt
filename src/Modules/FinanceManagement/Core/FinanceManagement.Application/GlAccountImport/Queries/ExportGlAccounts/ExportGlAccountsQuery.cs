using FinanceManagement.Application.GlAccountImport.Dto;
using MediatR;

namespace FinanceManagement.Application.GlAccountImport.Queries.ExportGlAccounts
{
    /// <summary>Full COA export for the session company. Re-imports cleanly (AC5).</summary>
    /// <param name="Format">"Excel" (default) or "Csv".</param>
    public sealed record ExportGlAccountsQuery(string? Format) : IRequest<GlAccountFileResultDto>;
}
