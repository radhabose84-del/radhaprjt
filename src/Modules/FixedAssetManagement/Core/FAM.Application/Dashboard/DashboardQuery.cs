using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Dashboard.Common;
using MediatR;

namespace FAM.Application.Dashboard
{
    public class DashboardQuery : IRequest<ChartDto>
    {
        public string? Type { get; set; }
         
         public int? DepartmentId { get; set; }
         
    }
}