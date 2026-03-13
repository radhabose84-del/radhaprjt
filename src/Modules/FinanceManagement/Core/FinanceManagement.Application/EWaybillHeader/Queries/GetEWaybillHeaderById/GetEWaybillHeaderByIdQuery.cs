using FinanceManagement.Application.EWaybillHeader.Dto;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Queries.GetEWaybillHeaderById
{
    public class GetEWaybillHeaderByIdQuery : IRequest<EWaybillHeaderDto?>
    {
        public int Id { get; set; }
    }
}
