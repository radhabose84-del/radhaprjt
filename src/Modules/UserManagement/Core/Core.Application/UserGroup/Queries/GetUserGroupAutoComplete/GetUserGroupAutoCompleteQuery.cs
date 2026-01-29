using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.UserGroup.Queries.GetUserGroupAutoComplete
{
    public class GetUserGroupAutoCompleteQuery : IRequest<List<UserGroupAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}