using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAccountingPeriodById
{
    public class GetAccountingPeriodByIdQuery : IRequest<AccountingPeriodDto?>
    {
        public int Id { get; set; }
    }
}
