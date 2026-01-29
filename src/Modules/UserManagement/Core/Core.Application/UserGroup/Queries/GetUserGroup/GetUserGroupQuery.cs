using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.UserGroup.Queries.GetUserGroup
{
    public class GetUserGroupQuery : IRequest<ApiResponseDTO<List<UserGroupDto>>>
   {
      public int PageNumber { get; set; }
      public int PageSize { get; set; } 
      public string? SearchTerm { get; set; }
   }
          
}