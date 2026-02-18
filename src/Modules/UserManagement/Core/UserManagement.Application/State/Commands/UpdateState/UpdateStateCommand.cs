using Contracts.Common;
using UserManagement.Application.State.Queries.GetStates;
using MediatR;

namespace UserManagement.Application.State.Commands.UpdateState
{
       public class UpdateStateCommand : IRequest<bool>
       {
              public int Id { get; set; }
              public string? StateCode { get; set; }
              public string? StateName { get; set; }
              public int CountryId { get; set; }    
              public byte IsActive { get; set; }      
         }
  
}