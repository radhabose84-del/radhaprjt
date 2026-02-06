using MediatR;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Application.Common.HttpResponse;

namespace UserManagement.Application.State.Commands.CreateState
{     
     public class CreateStateCommand : IRequest<StateDto>
     {
          public string? StateCode { get; set; }
          public string? StateName { get; set; }
          public int CountryId { get; set; }                
     }    

}