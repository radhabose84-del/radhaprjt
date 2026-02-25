using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;
using MediatR;

namespace FAM.Application.DepreciationDetail.Queries.GetDepreciationAbstract
{
    public class GetDepreciationAbstractQuery  : IRequest<List<DepreciationAbstractDto>>
    {
        public int companyId {get; set;   }
        public int unitId {get; set;   }
        public int finYearId {get; set;}
        public DateTimeOffset? startDate {get; set;}
        public DateTimeOffset? endDate {get; set;}
        public int depreciationType {get; set;}
        public int depreciationPeriod { get; set; }
    }
}