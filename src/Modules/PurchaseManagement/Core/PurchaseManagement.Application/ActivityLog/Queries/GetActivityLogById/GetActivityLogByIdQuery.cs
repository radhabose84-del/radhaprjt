using PurchaseManagement.Domain.Entities;

using MediatR;

public record GetActivityLogByIdQuery(long Id) : IRequest<ActivityLog?>;