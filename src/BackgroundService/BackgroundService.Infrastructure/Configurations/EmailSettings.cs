using System.Collections.Generic;

namespace BackgroundService.Infrastructure.Configurations
{
    public class EmailSettings
    {
        public Dictionary<string, EmailProviderSettings> Providers { get; set; } = new Dictionary<string, EmailProviderSettings>();
    }

    public class EmailProviderSettings
    {
        public string? Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
