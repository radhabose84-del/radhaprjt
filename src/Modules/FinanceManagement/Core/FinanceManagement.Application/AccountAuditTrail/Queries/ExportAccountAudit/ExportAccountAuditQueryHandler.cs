using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Contracts.Interfaces;
using FinanceManagement.Application.AccountAuditTrail.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountAuditTrail;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountAuditTrail.Queries.ExportAccountAudit
{
    public class ExportAccountAuditQueryHandler
        : IRequestHandler<ExportAccountAuditQuery, AccountAuditExportDto>
    {
        private readonly IAccountAuditTrailQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public ExportAccountAuditQueryHandler(
            IAccountAuditTrailQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<AccountAuditExportDto> Handle(
            ExportAccountAuditQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId() ?? 0;

            var rows = await _queryRepository.ExportAsync(
                companyId, request.From, request.To, request.EntityName, cancellationToken);

            var dto = new AccountAuditExportDto
            {
                EntityName = request.EntityName,
                FromDate = request.From,
                ToDate = request.To,
                RecordCount = rows.Count,        // record-count checksum (AC-4)
                Checksum = ComputeChecksum(rows), // SHA-256 over row contents (tamper-evident)
                Rows = rows
            };

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "ExportAccountAuditQuery",
                actionCode: "Export",
                actionName: dto.RecordCount.ToString(),
                details: $"Account audit exported: {dto.RecordCount} rows [{request.From:o} – {request.To:o}], checksum {dto.Checksum}.",
                module: "AccountAuditTrail"), cancellationToken);

            return dto;
        }

        // Order-independent-of-storage, content-bound hash: any added/removed/altered row changes it.
        private static string ComputeChecksum(IReadOnlyList<AccountAuditTrailDto> rows)
        {
            var sb = new StringBuilder();
            foreach (var r in rows)
            {
                sb.Append(r.Id).Append('|')
                  .Append(r.EntityName).Append('|')
                  .Append(r.EntityId).Append('|')
                  .Append(r.Action).Append('|')
                  .Append(r.PropertyName).Append('|')
                  .Append(r.OldValue ?? string.Empty).Append('|')
                  .Append(r.NewValue ?? string.Empty).Append('|')
                  .Append(r.CreatedBy?.ToString(CultureInfo.InvariantCulture) ?? string.Empty).Append('|')
                  .Append(r.CreatedByRole ?? string.Empty).Append('|')
                  .Append(r.CreatedDate.ToString("o", CultureInfo.InvariantCulture))
                  .Append('\n');
            }

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToHexString(hash);
        }
    }
}
