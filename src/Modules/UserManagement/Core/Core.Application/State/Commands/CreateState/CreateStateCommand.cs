using MediatR;
using Core.Application.State.Queries.GetStates;
using Core.Application.Common.HttpResponse;

namespace Core.Application.State.Commands.CreateState
{     
     public class CreateStateCommand : IRequest<StateDto>
     {
          public string? StateCode { get; set; }
          public string? StateName { get; set; }
          public int CountryId { get; set; }                
     }    

}