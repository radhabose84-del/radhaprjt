using Contracts.Common;
using MediatR;
using ProductionManagement.Application.YarnConversionHeader.Dto;

namespace ProductionManagement.Application.YarnConversionHeader.Queries.GetAllYarnConversionHeader
{
    public class GetAllYarnConversionHeaderQuery : IRequest<ApiResponseDTO<List<YarnConversionHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
