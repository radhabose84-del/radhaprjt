using FinanceManagement.Application.CoaChangeRequest.Dto;
using MediatR;

namespace FinanceManagement.Application.CoaChangeRequest.Queries.GetCoaUnfreezeRequestById
{
    public class GetCoaUnfreezeRequestByIdQuery : IRequest<CoaUnfreezeRequestDto?>
    {
        public int Id { get; set; }
    }
}
