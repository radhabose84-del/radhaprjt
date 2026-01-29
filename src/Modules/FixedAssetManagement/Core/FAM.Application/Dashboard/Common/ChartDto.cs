using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.Dashboard.Common
{
    public class ChartDto
    {
        public List<string>? Categories { get; set; }
        public List<ChartSeriesDto>? Series { get; set; }
    }
}