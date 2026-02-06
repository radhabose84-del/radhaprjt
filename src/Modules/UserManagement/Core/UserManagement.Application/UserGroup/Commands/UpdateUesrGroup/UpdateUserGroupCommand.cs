using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using MediatR;

namespace UserManagement.Application.UserGroup.Commands.UpdateUesrGroup
{
    public class UpdateUserGroupCommand : IRequest<bool>
       {
              public int Id { get; set; }
              public string? GroupCode { get; set; }
              public string? GroupName { get; set; }
              public byte IsActive { get; set; } 
         }
  
}