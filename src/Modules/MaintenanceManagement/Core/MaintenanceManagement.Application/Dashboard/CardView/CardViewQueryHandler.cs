using MaintenanceManagement.Application.Common.Interfaces.IDashboard;
using MaintenanceManagement.Application.Dashboard.CardView;
using MaintenanceManagement.Application.Dashboard.Common;
using MaintenanceManagement.Application.Dashboard.DashboardQuery;
using MediatR;

public class CardViewQueryHandler : IRequestHandler<CardViewQuery, CardViewDto>
{
    private readonly IDashboardQueryRepository _repository;

    public CardViewQueryHandler(IDashboardQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<CardViewDto> Handle(CardViewQuery request, CancellationToken cancellationToken)
    {       
        return await _repository.GetCardDashboardAsync(
            request.FromDate,
            request.ToDate,
            "CardView",
            request.DepartmentId,
            request.MachineGroupId
        );
    }
}
