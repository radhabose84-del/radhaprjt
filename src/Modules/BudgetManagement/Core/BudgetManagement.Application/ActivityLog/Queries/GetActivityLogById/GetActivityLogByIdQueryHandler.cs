using BudgetManagement.Domain.Entities;
using MediatR;

public sealed class GetActivityLogByIdQueryHandler
    : IRequestHandler<GetActivityLogByIdQuery, ActivityLog?>
{
    private readonly IActivityLogQueryRepository _repo;
    public GetActivityLogByIdQueryHandler(IActivityLogQueryRepository repo) => _repo = repo;

    public Task<ActivityLog?> Handle(GetActivityLogByIdQuery request, CancellationToken ct)
        => _repo.GetByIdAsync(request.Id, ct);
}