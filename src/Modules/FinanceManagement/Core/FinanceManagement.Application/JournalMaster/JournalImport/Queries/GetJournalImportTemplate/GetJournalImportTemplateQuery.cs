using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalImport.Queries.GetJournalImportTemplate
{
    // Returns the downloadable journal-import template (.xlsx).
    public sealed record GetJournalImportTemplateQuery : IRequest<JournalImportFileDto>;
}
