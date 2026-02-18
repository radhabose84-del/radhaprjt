using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using MediatR;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetAllProjectWBS
{
    public class GetAllprojectWBSQuery  : IRequest<ApiResponseDTO<List<ProjectWorkBreakdownStructureDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
    }
}