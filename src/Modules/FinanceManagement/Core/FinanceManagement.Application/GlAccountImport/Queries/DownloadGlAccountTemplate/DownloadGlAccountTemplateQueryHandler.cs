using FinanceManagement.Application.Common.Interfaces.IGlAccountImport;
using FinanceManagement.Application.GlAccountImport.Dto;
using FinanceManagement.Application.GlAccountImport.Services;
using MediatR;

namespace FinanceManagement.Application.GlAccountImport.Queries.DownloadGlAccountTemplate
{
    public class DownloadGlAccountTemplateQueryHandler
        : IRequestHandler<DownloadGlAccountTemplateQuery, GlAccountFileResultDto>
    {
        private readonly IGlAccountImportFileService _fileService;

        public DownloadGlAccountTemplateQueryHandler(IGlAccountImportFileService fileService)
        {
            _fileService = fileService;
        }

        public Task<GlAccountFileResultDto> Handle(
            DownloadGlAccountTemplateQuery request, CancellationToken cancellationToken)
        {
            var format = NormalizeFormat(request.Format);
            return Task.FromResult(_fileService.BuildTemplate(format));
        }

        private static string NormalizeFormat(string? format) =>
            string.Equals(format, GlAccountImportFileService.FormatCsv, StringComparison.OrdinalIgnoreCase)
                ? GlAccountImportFileService.FormatCsv
                : GlAccountImportFileService.FormatExcel;
    }
}
