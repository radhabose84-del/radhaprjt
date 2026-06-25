using FinanceManagement.Application.FinancialYearMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialYearMasterById
{
    public class GetFinancialYearMasterByIdQuery : IRequest<FinancialYearMasterDto?>
    {
        public int Id { get; set; }
    }
}
