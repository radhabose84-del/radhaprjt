
using FAM.Application.Common.HttpResponse;
using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using MediatR;

namespace FAM.Application.WDVDepreciation.Commands.CreateDepreciation
{
    public class CreateDepreciationCommand  : IRequest<CalculationDepreciationDto>
    {      
        public int FinYearId { get; set; }     
    }
}