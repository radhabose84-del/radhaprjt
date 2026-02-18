
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IReports;
using MediatR;

namespace FAM.Application.Reports.AssetReport
{
    public class AssetReportQueryHandler  : IRequestHandler<AssetReportQuery, ApiResponseDTO<List<AssetReportDto>>>
    {
        
        private readonly IReportRepository _reportQueryRepository;
        private readonly IMapper _mapper;
        public AssetReportQueryHandler( IReportRepository reportQueryRepository, IMapper mapper)
        {
            _reportQueryRepository = reportQueryRepository;
            _mapper = mapper;
        }      
        public async Task<ApiResponseDTO<List<AssetReportDto>>> Handle(AssetReportQuery request, CancellationToken cancellationToken)
        {
            var reportEntities = await _reportQueryRepository.AssetReportAsync(request.FromDate,request.ToDate) ?? new List<AssetReportDto>();             
            var reportDto = _mapper.Map<List<AssetReportDto>>(reportEntities) ?? new List<AssetReportDto>();
         
            return new ApiResponseDTO<List<AssetReportDto>>
            {
                IsSuccess = reportDto.Any(),
                Message = reportDto.Any()
                ? "Asset Report retrieved successfully."
                : "No Asset Report found.",
                Data = reportDto
            };
        }
    }
}
    