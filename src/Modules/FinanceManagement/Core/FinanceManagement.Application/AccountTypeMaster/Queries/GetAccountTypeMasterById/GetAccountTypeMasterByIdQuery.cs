using FinanceManagement.Application.AccountTypeMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountTypeMaster.Queries.GetAccountTypeMasterById
{
    public class GetAccountTypeMasterByIdQuery : IRequest<AccountTypeMasterDto?>
    {
        public int Id { get; set; }
    }
}
