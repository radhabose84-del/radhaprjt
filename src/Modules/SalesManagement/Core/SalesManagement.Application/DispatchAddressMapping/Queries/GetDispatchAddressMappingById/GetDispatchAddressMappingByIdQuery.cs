using MediatR;
using SalesManagement.Application.DispatchAddressMapping.Dto;

namespace SalesManagement.Application.DispatchAddressMapping.Queries.GetDispatchAddressMappingById
{
    public class GetDispatchAddressMappingByIdQuery : IRequest<DispatchAddressMappingDto>
    {
        public int Id { get; set; }
    }
}
