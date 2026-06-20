using FinanceManagement.Application.ProfitCentre.Dto;
using MediatR;

namespace FinanceManagement.Application.ProfitCentre.Queries.GetProfitCentreById
{
    public class GetProfitCentreByIdQuery : IRequest<ProfitCentreDto?>
    {
        public int Id { get; set; }
    }
}
