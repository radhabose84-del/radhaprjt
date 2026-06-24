using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetJournalPrint
{
    public class GetJournalPrintQueryHandler : IRequestHandler<GetJournalPrintQuery, JournalPrintDto?>
    {
        private readonly IJournalQueryRepository _queryRepository;
        private readonly ICompanyLookup _companyLookup;
        private readonly IMediator _mediator;

        public GetJournalPrintQueryHandler(IJournalQueryRepository queryRepository, ICompanyLookup companyLookup, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _companyLookup = companyLookup;
            _mediator = mediator;
        }

        public async Task<JournalPrintDto?> Handle(GetJournalPrintQuery request, CancellationToken cancellationToken)
        {
            var h = await _queryRepository.GetByIdAsync(request.Id);
            if (h == null)
                return null;

            var dto = new JournalPrintDto
            {
                CompanyId = h.CompanyId,
                CompanyName = h.CompanyName,
                UnitId = h.UnitId,
                UnitName = h.UnitName,
                Id = h.Id,
                VoucherNo = h.VoucherNo,
                VoucherDate = h.VoucherDate,
                PostingDate = h.PostingDate,
                StatusName = h.StatusName,
                Narration = h.Narration,
                TotalDr = h.TotalDr,
                TotalCr = h.TotalCr,
                PreparedByName = h.CreatedByName,
                PreparedAt = h.CreatedDate,
                ApprovedByName = h.ApprovedBy,
                ApprovedAt = h.ApprovedAt,
                VerifyUrl = string.IsNullOrEmpty(h.VoucherNo) ? null : $"/fin/jv/{h.VoucherNo}",
                Lines = (h.Lines ?? new List<JournalDetailDto>())
                    .OrderBy(l => l.LineNo)
                    .Select(l => new JournalPrintLineDto
                    {
                        LineNo = l.LineNo,
                        GlAccountId = l.GlAccountId,
                        AccountCode = l.GlAccountCode,
                        AccountName = l.GlAccountName,
                        CostCentreId = l.CostCentreId,
                        CostCentreName = l.CostCentreName,
                        DrAmount = l.DrAmount,
                        CrAmount = l.CrAmount,
                        LineNarration = l.LineNarration
                    }).ToList()
            };

            // Company legal name + GSTIN from the cross-module lookup (cached).
            var companies = await _companyLookup.GetAllCompanyAsync();
            var company = companies.FirstOrDefault(c => c.CompanyId == h.CompanyId);
            if (company != null)
            {
                dto.CompanyName ??= company.CompanyName;
                dto.CompanyLegalName = company.LegalName;
                dto.CompanyGstin = company.GstNumber;
            }

            dto.Fingerprint = ComputeFingerprint(dto);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Print",
                actionCode: "JOURNAL_PRINT",
                actionName: dto.VoucherNo ?? dto.Id.ToString(),
                details: $"Journal voucher {dto.Id} print model generated.",
                module: "Journal"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }

        // Deterministic 16-hex content seal — identical content always yields the same fingerprint, so a
        // re-print of a sealed (posted) voucher reproduces the exact document.
        private static string ComputeFingerprint(JournalPrintDto d)
        {
            var sb = new StringBuilder();
            sb.Append(d.Id).Append('|').Append(d.VoucherNo).Append('|')
              .Append(d.VoucherDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append('|')
              .Append(d.PostingDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append('|')
              .Append(d.TotalDr.ToString("0.00", CultureInfo.InvariantCulture)).Append('|')
              .Append(d.TotalCr.ToString("0.00", CultureInfo.InvariantCulture)).Append('|');
            foreach (var l in d.Lines)
            {
                sb.Append(l.LineNo).Append(':').Append(l.GlAccountId).Append(':')
                  .Append(l.DrAmount.ToString("0.00", CultureInfo.InvariantCulture)).Append(':')
                  .Append(l.CrAmount.ToString("0.00", CultureInfo.InvariantCulture)).Append(';');
            }

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToHexString(hash, 0, 8).ToLowerInvariant();   // 16 hex chars
        }
    }
}
