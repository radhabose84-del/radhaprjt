using MediatR;

namespace UserManagement.Application.Units.Commands.DeleteUnit
{
    public class DeleteUnitCommand : IRequest<int>
    {
            public int UnitId { get; set; }
    }
 
    }
    
