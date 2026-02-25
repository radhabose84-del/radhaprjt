#nullable disable
namespace Contracts.Dtos.Email
{
    public class MailSettings
{
    public Dictionary<string, EmailProvider> Providers { get; set; }
}

public class EmailProvider
{    
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
}
}