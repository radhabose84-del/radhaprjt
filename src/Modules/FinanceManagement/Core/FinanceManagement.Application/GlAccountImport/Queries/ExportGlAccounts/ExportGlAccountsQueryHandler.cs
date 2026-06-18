using Contracts.Interfaces;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IGlAccountImport;
using FinanceManagement.Application.GlAccountImport.Dto;
using FinanceManagement.Application.GlAccountImport.Services;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountImport.Queries.ExportGlAccounts
{
    public class ExportGlAccountsQueryHandler
        : IRequestHandler<ExportGlAccountsQuery, GlAccountFileResultDto>
    {
        private readonly IGlAccountImportQueryRepository _queryRepository;
        private readonly IGlAccountImportFileService _fileService;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public ExportGlAccountsQueryHandler(
            IGlAccountImportQueryRepository queryRepository,
            IGlAccountImportFileService fileService,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _fileService = fileService;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<GlAccountFileResultDto> Handle(
            ExportGlAccountsQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var format = NormalizeFormat(request.Format);
            var rows = await _queryRepository.GetExportRowsAsync(companyId, cancellationToken);
            var file = _fileService.BuildExport(rows, format);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Export",
                actionCode: "GL_ACCOUNT_EXPORT",
                actionName: rows.Count.ToString(),
                details: $"COA export ({format}) of {rows.Count} row(s) generated for Company {companyId}.",
                module: "GlAccountImport");
            await _mediator.Publish(auditEvent, cancellationToken);

            return file;
        }

        private static string NormalizeFormat(string? format) =>
            string.Equals(format, GlAccountImportFileService.FormatCsv, StringComparison.OrdinalIgnoreCase)
                ? GlAccountImportFileService.FormatCsv
                : GlAccountImportFileService.FormatExcel;
    }
}
