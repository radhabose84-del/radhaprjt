using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.UserGroup.Queries.GetUserGroup
{
    public class GetUserGroupQuery : IRequest<ApiResponseDTO<List<UserGroupDto>>>
   {
      public int PageNumber { get; set; }
      public int PageSize { get; set; } 
      public string? SearchTerm { get; set; }
   }
          
}