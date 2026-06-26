using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetScheduleIIIImportTemplate
{
    public sealed record GetScheduleIIIImportTemplateQuery : IRequest<ScheduleIIIImportFileDto>;
}
