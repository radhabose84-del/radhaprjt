using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.UserGroup.Queries.GetUserGroupAutoComplete
{
    public class GetUserGroupAutoCompleteQuery : IRequest<List<UserGroupAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}