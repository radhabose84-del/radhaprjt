
using System.Net.Http.Json;
using Core.Application.Common.Interfaces;
using Contracts.Events.Notifications;
using Microsoft.AspNetCore.Http;

public class EmailSenderService  : IEmailService
{
   private readonly IHttpClientFactory _httpClientFactory;
//    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmailSenderService(IHttpClientFactory httpClientFactory
    // , IHttpContextAccessor httpContextAccessor
    )
    {
        _httpClientFactory = httpClientFactory;
        // _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> SendEmailAsync(SendEmailCommand command)
    {
        // var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

        //     if (string.IsNullOrEmpty(token))
        //     {
        //         throw new Exception("No Authorization token found in the current context.");
        //     }
        //     //  ✅ Ensure it has "Bearer " prefix
        //     if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        //     {

        //         token = $"Bearer {token}";
        //     }

             
        // _logger.LogInformation("✅ from EmailSenderService");    
        var client = _httpClientFactory.CreateClient("BackgroundServiceClient");
        
        // client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
        var response = await client.PostAsJsonAsync("api/email/send", command);
        return response.IsSuccessStatusCode;
    }
}    

