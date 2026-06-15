using FinanceManagement.Application.AccountGroup.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupById
{
    public class GetAccountGroupByIdQuery : IRequest<AccountGroupDetailDto?>
    {
        public int Id { get; set; }
    }
}
