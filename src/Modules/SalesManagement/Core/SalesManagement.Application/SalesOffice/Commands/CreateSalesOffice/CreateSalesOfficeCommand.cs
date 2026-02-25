using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOffice.Commands.CreateSalesOffice
{
    public class CreateSalesOfficeCommand : IRequest<ApiResponseDTO<int>>
    {
        public string SalesOfficeName { get; set; } = null!;
        public int SalesOrganisationId { get; set; }
        public int CityId { get; set; }
        public string Pincode { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string ResponsibleManager { get; set; } = null!;
        public string RegionTerritory { get; set; } = null!;
        public string Address { get; set; } = null!;
    }
}
