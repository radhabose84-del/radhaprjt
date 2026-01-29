using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces.IHangfire;
using Grpc.Core;
using GrpcServices.HangfireDelete;
using Hangfire;

namespace BackgroundService.API.GrpcServices
{
    public class MaintenanceHangfireRemoveGrpcService : MaintenanceHangfireDeleteService.MaintenanceHangfireDeleteServiceBase
    {
        private readonly IHangfireQuery _hangfireQuery;
        public MaintenanceHangfireRemoveGrpcService(IHangfireQuery hangfireQuery)
        {
            _hangfireQuery = hangfireQuery;
        }
        public async override Task<HangfireResponse> HangfireRemove(HangfireRequest request, ServerCallContext context)
        {

                var hangfireJob = await _hangfireQuery.GetHangfireJobByTransactionId(Convert.ToInt32(request.HangfireJobId));

                foreach (var id in hangfireJob)
                {
                    BackgroundJob.Delete(id.ToString());
                }
            
                var response = new HangfireResponse
               {
                   IsSuccess = true
               };

                return response;
            
        }
        
    }
}