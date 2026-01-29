using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Email
{
    public class MailSettings
{
    public Dictionary<string, EmailProvider> Providers { get; set; }
}

public class EmailProvider
{    
    public string Host { get; set; }
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }    
}
}