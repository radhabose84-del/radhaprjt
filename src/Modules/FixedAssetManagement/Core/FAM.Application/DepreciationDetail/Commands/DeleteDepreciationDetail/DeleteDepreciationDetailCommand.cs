
using Contracts.Common;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;
using MediatR;

namespace FAM.Application.DepreciationDetail.Commands.DeleteDepreciationDetail
{
    public class DeleteDepreciationDetailCommand :  IRequest<DepreciationDto>  
    {
        public int companyId { get; set; } 
        public int unitId { get; set; } 
        public int finYearId { get; set; }        
        public int depreciationType { get; set; }      
        public int depreciationPeriod { get; set; }  
    }
}