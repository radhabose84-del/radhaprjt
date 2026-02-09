
using BudgetManagement.Domain.Entities;
using MediatR;


public sealed class GetActivityLogsQueryHandler
    : IRequestHandler<GetActivityLogsQuery, (List<ActivityLog>, int)>
{
    private readonly IActivityLogQueryRepository _repo;
    public GetActivityLogsQueryHandler(IActivityLogQueryRepository repo) => _repo = repo;

    public Task<(List<ActivityLog>, int)> Handle(GetActivityLogsQuery request, CancellationToken ct)
        => _repo.GetAllAsync(request.EntityName, request.EntityId, request.PageNumber, request.PageSize, ct);
}
