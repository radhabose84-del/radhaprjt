using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOffice.Commands.UpdateSalesOffice
{
    public class UpdateSalesOfficeCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string SalesOfficeName { get; set; } = null!;
        public int SalesOrganisationId { get; set; }
        public int CityId { get; set; }
        public string Pincode { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string ResponsibleManager { get; set; } = null!;
        public string RegionTerritory { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int IsActive { get; set; }
    }
}
