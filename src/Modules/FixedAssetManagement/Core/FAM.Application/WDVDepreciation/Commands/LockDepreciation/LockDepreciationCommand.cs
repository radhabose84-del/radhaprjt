
using Contracts.Common;
using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using MediatR;

namespace FAM.Application.WDVDepreciation.Commands.LockDepreciation
{
    public class LockDepreciationCommand  : IRequest<CalculationDepreciationDto>
    {      
        public int FinYearId { get; set; }     
    }
}