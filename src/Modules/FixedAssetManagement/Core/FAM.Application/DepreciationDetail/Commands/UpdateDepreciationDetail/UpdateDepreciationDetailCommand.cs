using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;
using MediatR;

namespace FAM.Application.DepreciationDetail.Commands.UpdateDepreciationDetail
{
    public class UpdateDepreciationDetailCommand  :  IRequest<DepreciationDto>
    {
        public int companyId { get; set; } 
        public int unitId { get; set; } 
        public int finYearId { get; set; }        
        public int depreciationType { get; set; }      
        public int depreciationPeriod { get; set; }  
    }
}