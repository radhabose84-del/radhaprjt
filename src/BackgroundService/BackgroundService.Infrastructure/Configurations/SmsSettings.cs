using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Infrastructure.Configurations
{
    public class SmsSettings
    {
        public string? Provider { get; set; }  
        public string? ApiKey { get; set; }  
        public string? BaseUrl { get; set; } 
        public string? Sender { get; set; } 
        public string? Route { get; set; } 
        public string? TemplateId { get; set; } 
        public string? adminMailId { get; set; }
    }
}