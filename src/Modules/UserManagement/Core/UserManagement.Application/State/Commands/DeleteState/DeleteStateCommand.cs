using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.State.Queries.GetStates;
using MediatR;

namespace UserManagement.Application.State.Commands.DeleteState
{
       public class DeleteStateCommand :  IRequest<StateDto>
       {
                public int Id { get; set; }                
       }
    
}