using FinanceManagement.Application.CostCentre.Dto;
using MediatR;

namespace FinanceManagement.Application.CostCentre.Queries.GetCostCentreById
{
    public class GetCostCentreByIdQuery : IRequest<CostCentreDto?>
    {
        public int Id { get; set; }
    }
}
