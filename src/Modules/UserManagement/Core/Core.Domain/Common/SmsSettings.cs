using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Domain.Common
{
  public class SmsSettings
    {
        public string? Provider { get; set; }  // Example: "externalapi"
        public string? ApiKey { get; set; }  // API Key for the SMS provider
        public string? BaseUrl { get; set; } // SMS API Base URL
        public string? Sender { get; set; } // Sender ID or Name
        public string? Route { get; set; } // Route (if required)
        public string? TemplateId { get; set; } // Template ID (if required)
        public string? adminmailid { get; set; }
    }
}