using FAM.Application.Common.Interfaces.IDashboard;
using MediatR;

namespace FAM.Application.Dashboard.CardView
{
    public class CardViewQueryHandler : IRequestHandler<CardViewQuery, CardViewDto>
    {

        public readonly IDashboardQueryRepository _dashboardQueryRepository;

        public CardViewQueryHandler  (IDashboardQueryRepository dashboardQueryRepository)
        {
            _dashboardQueryRepository = dashboardQueryRepository;
        }

        public  async Task<CardViewDto> Handle(CardViewQuery request, CancellationToken cancellationToken)
        {
          return await _dashboardQueryRepository.GetDashboardDataAsync( );
        }
    }


}
