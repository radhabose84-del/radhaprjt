using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalImport.Queries.GetJournalImportTemplate
{
    public class GetJournalImportTemplateQueryHandler : IRequestHandler<GetJournalImportTemplateQuery, JournalImportFileDto>
    {
        private readonly IJournalImportFileService _fileService;

        public GetJournalImportTemplateQueryHandler(IJournalImportFileService fileService)
        {
            _fileService = fileService;
        }

        public Task<JournalImportFileDto> Handle(GetJournalImportTemplateQuery request, CancellationToken cancellationToken) =>
            Task.FromResult(_fileService.BuildTemplate());
    }
}
