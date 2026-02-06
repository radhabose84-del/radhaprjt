using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
// using MaintenanceManagement.Application.Common.Interfaces.IBackgroundService;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands;

namespace MaintenanceManagement.Infrastructure.Services
{
    // public class BackgroundServiceClient : IBackgroundServiceClient
    // {
    //     private readonly IHttpClientFactory _httpClientFactory;
    //     public BackgroundServiceClient(IHttpClientFactory httpClientFactory)
    //     {
    //         _httpClientFactory= httpClientFactory;
    //     }
    //     public async Task<string> ScheduleWorkOrder(int PreventiveScheduleId, int delayInMinutes)
    //     {
    //         var client = _httpClientFactory.CreateClient("BackgroundServiceClient");

    //               var request = new ScheduleWorkOrderBackgroundCommand
    //           {
    //               PreventiveScheduleId = PreventiveScheduleId,
    //               DelayInMinutes = delayInMinutes
    //           };
    //         var response = await client.PostAsJsonAsync("/api/maintenancehangfirejobs/scheduleWorkOrder", request);
    //         var result = await response.Content.ReadFromJsonAsync<MaintenanceJobResponseDto>();
    //         //response.EnsureSuccessStatusCode();
    //         return result?.JobId ?? string.Empty;
    //     }
    // }
}