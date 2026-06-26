using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetScheduleIIIImportTemplate
{
    public class GetScheduleIIIImportTemplateQueryHandler : IRequestHandler<GetScheduleIIIImportTemplateQuery, ScheduleIIIImportFileDto>
    {
        private readonly IScheduleIIIImportFileService _fileService;

        public GetScheduleIIIImportTemplateQueryHandler(IScheduleIIIImportFileService fileService)
        {
            _fileService = fileService;
        }

        public Task<ScheduleIIIImportFileDto> Handle(GetScheduleIIIImportTemplateQuery request, CancellationToken cancellationToken) =>
            Task.FromResult(_fileService.BuildTemplate());
    }
}
