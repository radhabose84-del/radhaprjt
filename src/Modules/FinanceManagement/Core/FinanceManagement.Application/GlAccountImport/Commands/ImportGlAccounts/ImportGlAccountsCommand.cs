using Contracts.Common;
using FinanceManagement.Application.GlAccountImport.Dto;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FinanceManagement.Application.GlAccountImport.Commands.ImportGlAccounts
{
    public class ImportGlAccountsCommand : IRequest<ApiResponseDTO<GlAccountImportResultDto>>
    {
        public IFormFile? File { get; set; }

        /// <summary>"AllOrNothing" (default) or "ValidRowsOnly".</summary>
        public string? Mode { get; set; }
    }

    public static class GlAccountImportModes
    {
        public const string AllOrNothing = "AllOrNothing";
        public const string ValidRowsOnly = "ValidRowsOnly";

        public static string Normalize(string? mode) =>
            string.Equals(mode, ValidRowsOnly, StringComparison.OrdinalIgnoreCase)
                ? ValidRowsOnly
                : AllOrNothing;
    }
}
