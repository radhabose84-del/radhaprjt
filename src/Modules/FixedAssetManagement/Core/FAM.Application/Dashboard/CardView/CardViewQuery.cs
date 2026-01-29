using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace FAM.Application.Dashboard.CardView
{
    public class CardViewQuery : IRequest<CardViewDto>
    {
        public string? Type { get; set; }

        public int? DepartmentId { get; set; }
    }
}