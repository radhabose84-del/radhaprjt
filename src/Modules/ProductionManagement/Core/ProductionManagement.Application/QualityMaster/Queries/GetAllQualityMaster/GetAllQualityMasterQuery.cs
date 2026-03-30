using Contracts.Common;
using MediatR;
using ProductionManagement.Application.QualityMaster.Dto;

namespace ProductionManagement.Application.QualityMaster.Queries.GetAllQualityMaster
{
    public class GetAllQualityMasterQuery : IRequest<ApiResponseDTO<List<QualityMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
