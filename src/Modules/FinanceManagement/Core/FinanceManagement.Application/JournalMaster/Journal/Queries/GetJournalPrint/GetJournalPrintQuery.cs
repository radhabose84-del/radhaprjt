using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetJournalPrint
{
    // US-GL01-18 — full print/PDF model for a single voucher (header + lines + maker/checker + fingerprint).
    public sealed record GetJournalPrintQuery(int Id) : IRequest<JournalPrintDto?>;
}
