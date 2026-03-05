using MediatR;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceById
{
    public class GetDispatchAdviceByIdQuery : IRequest<DispatchAdviceHeaderDto?>
    {
        public int Id { get; set; }
    }
}
