using Core.Application.Common.HttpResponse;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.UserRole.Queries.GetRole
{
    public class GetRoleQuery : IRequest<ApiResponseDTO<List<GetUserRoleDto>>>
    {

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }

        
    }
}