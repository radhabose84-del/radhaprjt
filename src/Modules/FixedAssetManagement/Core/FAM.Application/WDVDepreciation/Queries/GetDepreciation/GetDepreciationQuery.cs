
using Contracts.Common;
using FAM.Application.WDVDepreciation.Queries.CalculateDepreciation;
using MediatR;

namespace FAM.Application.WDVDepreciation.Queries.GetDepreciation
{
    public class GetDepreciationQuery  : IRequest<List<CalculationDepreciationDto>>
    {      
        public int FinYearId { get; set; }     
    }
}