using FinanceManagement.Application.GlAccountImport.Dto;
using MediatR;

namespace FinanceManagement.Application.GlAccountImport.Queries.DownloadGlAccountTemplate
{
    /// <param name="Format">"Excel" (default) or "Csv".</param>
    public sealed record DownloadGlAccountTemplateQuery(string? Format) : IRequest<GlAccountFileResultDto>;
}
