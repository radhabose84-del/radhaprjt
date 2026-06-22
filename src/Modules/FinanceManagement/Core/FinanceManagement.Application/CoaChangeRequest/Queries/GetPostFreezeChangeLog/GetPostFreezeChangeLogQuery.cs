using FinanceManagement.Application.CoaChangeRequest.Dto;
using MediatR;

namespace FinanceManagement.Application.CoaChangeRequest.Queries.GetPostFreezeChangeLog
{
    // US-GL02-08B (AC3) — the post-freeze change log for the session company.
    public class GetPostFreezeChangeLogQuery : IRequest<List<PostFreezeChangeLogDto>>
    {
    }
}
