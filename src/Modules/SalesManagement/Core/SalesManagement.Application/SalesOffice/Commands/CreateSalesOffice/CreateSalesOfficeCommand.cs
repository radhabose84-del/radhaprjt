using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOffice.Commands.CreateSalesOffice
{
    public class CreateSalesOfficeCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? SalesOfficeName { get; set; }
        public int SalesOrganisationId { get; set; }
        public int CityId { get; set; }
        public string? Pincode { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ResponsibleManager { get; set; }
        public string? RegionTerritory { get; set; }
        public string? Address { get; set; }
    }
}
