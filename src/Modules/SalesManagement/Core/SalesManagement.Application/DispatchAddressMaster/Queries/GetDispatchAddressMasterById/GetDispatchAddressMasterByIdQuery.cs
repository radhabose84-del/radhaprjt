using MediatR;
using SalesManagement.Application.DispatchAddressMaster.Dto;

namespace SalesManagement.Application.DispatchAddressMaster.Queries.GetDispatchAddressMasterById
{
    public class GetDispatchAddressMasterByIdQuery : IRequest<DispatchAddressMasterDto?>
    {
        public int Id { get; set; }
    }
}
