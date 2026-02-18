using Contracts.Common;
using UserManagement.Application.Units.Queries.GetUnits;
using MediatR;

namespace UserManagement.Application.Units.Commands.UpdateUnit
{
    public class UpdateUnitCommand : IRequest<int>
    {    
    //public int UnitId  { get; set; }
    public UpdateUnitsDto? UpdateUnitDto { get; set; }  
    }
}