using Contracts.Common;
using MediatR;

namespace FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail
{
    public class GetDepreciationDetailQuery : IRequest<ApiResponseDTO<List<DepreciationDto>>>
    {
        public int companyId {get; set;   }
        public int unitId {get; set;   }
        public int finYearId {get; set;}
        public DateTimeOffset? startDate {get; set;}
        public DateTimeOffset? endDate {get; set;}
        public int depreciationType {get; set;}
        public int PageNumber { get; set; }
        public int PageSize { get; set; } 
        public string? SearchTerm { get; set; }
        public int depreciationPeriod { get; set; }
    }
}