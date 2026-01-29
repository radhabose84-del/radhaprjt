using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.State.Queries.GetStates
{   
   public class GetStateQuery : IRequest<ApiResponseDTO<List<StateDto>>>
   {
      public int PageNumber { get; set; }
      public int PageSize { get; set; } 
      public string? SearchTerm { get; set; }
   }
          
}