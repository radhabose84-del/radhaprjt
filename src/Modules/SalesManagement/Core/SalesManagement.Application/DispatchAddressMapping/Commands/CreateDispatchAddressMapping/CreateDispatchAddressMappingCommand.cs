using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.DispatchAddressMapping.Commands.CreateDispatchAddressMapping
{
    public class CreateDispatchAddressMappingCommand : IRequest<ApiResponseDTO<int>>
    {
        public int PartyId { get; set; }
        public int DispatchAddressId { get; set; }
        public int UsageTypeId { get; set; }
        public bool IsDefault { get; set; }
    }
}
