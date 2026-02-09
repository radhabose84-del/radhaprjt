using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using MediatR;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetById
{
    public class GetProjectWorkBreakdownStructureByIdQuery : IRequest<ProjectWorkBreakdownStructureDto?>
    {
        public int Id { get; set; }

        public GetProjectWorkBreakdownStructureByIdQuery(int id)
        {
            Id = id;
        }
    }
}