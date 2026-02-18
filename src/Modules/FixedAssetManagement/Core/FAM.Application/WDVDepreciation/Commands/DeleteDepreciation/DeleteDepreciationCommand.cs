
using Contracts.Common;
using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using MediatR;

namespace FAM.Application.WDVDepreciation.Commands.DeleteDepreciation
{
    public class DeleteDepreciationCommand  : IRequest<CalculationDepreciationDto>
    {      
        public int FinYearId { get; set; }     
    }
}