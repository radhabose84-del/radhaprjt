using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.UserGroup.Queries.GetUserGroupAutoComplete
{
    public class GetUserGroupAutoCompleteQuery : IRequest<List<UserGroupAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}