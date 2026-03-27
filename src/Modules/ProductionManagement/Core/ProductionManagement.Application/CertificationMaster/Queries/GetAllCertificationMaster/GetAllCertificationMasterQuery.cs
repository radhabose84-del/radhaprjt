using Contracts.Common;
using MediatR;
using ProductionManagement.Application.CertificationMaster.Dto;

namespace ProductionManagement.Application.CertificationMaster.Queries.GetAllCertificationMaster
{
    public class GetAllCertificationMasterQuery : IRequest<ApiResponseDTO<List<CertificationMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
