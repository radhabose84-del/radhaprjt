using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.DispatchAddressMaster.Commands.UpdateDispatchAddressMaster
{
    public class UpdateDispatchAddressMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? DispatchAddressName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string? PinCode { get; set; }
        public string? ContactPerson { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public string? GSTIN { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? FreightId { get; set; }
        public int IsActive { get; set; }
    }
}
