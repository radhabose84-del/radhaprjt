using BudgetManagement.Domain.Entities;
using MediatR;


public record GetActivityLogsQuery(string EntityName, int EntityId, int PageNumber = 1, int PageSize = 50)
    : IRequest<(List<ActivityLog> Items, int Total)>;
