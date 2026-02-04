using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Units.Queries.GetUnits;
using MediatR;

namespace UserManagement.Application.Units.Commands.DeleteUnit
{
    public class DeleteUnitCommand : IRequest<int>
    {
            public int UnitId { get; set; }
    }
 
    }
    
