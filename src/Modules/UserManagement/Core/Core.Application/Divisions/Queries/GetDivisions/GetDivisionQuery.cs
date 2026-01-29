using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using System.Text;
using Core.Application.Common.HttpResponse;

namespace Core.Application.Divisions.Queries.GetDivisions
{
    public class GetDivisionQuery : IRequest<ApiResponseDTO<List<DivisionDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}