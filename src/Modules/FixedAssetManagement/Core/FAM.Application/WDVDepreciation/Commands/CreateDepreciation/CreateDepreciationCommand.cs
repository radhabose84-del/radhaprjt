using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using MediatR;
using Contracts.Common;

namespace FAM.Application.WDVDepreciation.Commands.CreateDepreciation
{
    public class CreateDepreciationCommand  : IRequest<CalculationDepreciationDto>, IRequirePermission
    {      
        public int FinYearId { get; set; }     
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
