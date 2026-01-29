using BackgroundService.Application.Interfaces;
using BackgroundService.Application.Interfaces.IHangfire;
using BackgroundService.Application.Jobhistory;
using BackgroundService.Infrastructure.Services;
using Grpc.Core;
using GrpcServices.Background; // This is from generated proto
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackgroundService.API.GrpcServices
{
    public class MaintenanceJobGrpcService : MaintenanceJobService.MaintenanceJobServiceBase
    {
        private readonly ILogger<MaintenanceJobGrpcService> _logger;
        private readonly IHangfireQuery _hangfireQuery;

        public MaintenanceJobGrpcService(ILogger<MaintenanceJobGrpcService> logger, IHangfireQuery hangfireQuery)
        {
            _logger = logger;
            _hangfireQuery = hangfireQuery;

        }

        public override async Task<ScheduleWorkOrderResponse> ScheduleWorkOrder(ScheduleWorkOrderRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Received request to schedule PreventiveScheduleId: {Id}",
                request.PreventiveScheduleId);

                var hangfireJob = await _hangfireQuery.GetHangfireJobByTransactionId(request.PreventiveScheduleId);

                foreach (var id in hangfireJob)
                {
                    BackgroundJob.Delete(id.ToString());
                }

            string jobId = BackgroundJob.Schedule<MaintenanceService>(
                job => job.SchedulerWorkOrderExecute(request.PreventiveScheduleId),
                TimeSpan.FromMinutes(request.DelayInMinutes)
            );
         
                

            _logger.LogInformation("Scheduled Hangfire Job ID: {JobId}", jobId);

            return new ScheduleWorkOrderResponse
            {
                JobId = jobId
            };
        }
    }
}
