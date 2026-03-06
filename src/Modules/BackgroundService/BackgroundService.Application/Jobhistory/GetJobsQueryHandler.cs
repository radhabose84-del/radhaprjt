using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces;
using MediatR;

namespace BackgroundService.Application.Jobhistory
{
    public class GetJobsQueryHandler : IRequestHandler<GetJobsQuery, JobHistoryDto>
    {
        private readonly IMaintenance _maintenance;
        public GetJobsQueryHandler(IMaintenance maintenance)
        {
            _maintenance = maintenance;
        }
        public async Task<JobHistoryDto> Handle(GetJobsQuery request, CancellationToken cancellationToken)
        {
            int totalJobs = await _maintenance.GetTotalPendingJobsAsync();

            return new JobHistoryDto
            {
                TotalJobs = totalJobs
            };
        }
    }
}