using System.Net;
using System.Security.Claims;
using Core.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Infrastructure.Repositories
{  public class IPAddressService : IIPAddressService
    {
         private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor
        public IPAddressService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

         public string GetSystemIPAddress()
        {
            string ipAddress = "127.0.0.1"; // Default to localhost.
            try
            {
                ipAddress = Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .FirstOrDefault(ip => ip.AddressFamily is System.Net.Sockets.AddressFamily.InterNetwork)?
                    .ToString() ?? ipAddress;
            }
            catch
            {
                
            }
            return ipAddress;
        }     
        public string GetUserBrowserDetails(string userAgent)
        {
            string os = ExtractOS(userAgent);
            string systemName = Environment.MachineName; // Get the system/machine name
            string browserAndVersion = ExtractBrowserAndVersion(userAgent);
            string ipAddress = GetSystemIPAddress();            
            return $"{os}/{systemName}/{browserAndVersion}/{ipAddress}";
        }
        private string ExtractOS(string userAgent)
        {
             if (userAgent.Contains("PostmanRuntime"))
            {
                return "PostmanOS"; // Default for Postman requests
            }
            if (userAgent.Contains("Windows NT")) return "WinNT";
            if (userAgent.Contains("Mac OS X")) return "MacOS";
            if (userAgent.Contains("Linux")) return "Linux";
            if (userAgent.Contains("Android")) return "Android";
            if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
            return "UnknownOS";
        }
        private string ExtractBrowserAndVersion(string userAgent)
        {
            if (userAgent.Contains("PostmanRuntime"))
            {
                var versionMatch = System.Text.RegularExpressions.Regex.Match(userAgent, @"PostmanRuntime/([\d\.]+)");
                if (versionMatch.Success)
                {
                    return $"Postman/{versionMatch.Groups[1].Value}";
                }
                return "Postman/Unknown";
            }
            
            var match = System.Text.RegularExpressions.Regex.Match(userAgent,
           @"(Chrome|Firefox|Safari|Edge|MSIE|Trident)[/ ]([\d\.]+)");

            if (match.Success)
            {
            string browser = match.Groups[1].Value;
            string version = match.Groups[2].Value;

            // Normalize Trident/IE versions
            if (browser is "Trident")
            {
            browser = "IE";
            version = "11.0"; // Default for Trident
            }

            return $"{browser}/{version}";
            }

            return "UnknownBrowser/0.0";
        }        
        public string GetUserIPAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        }

        public string GetUserAgent()
        {
            var httpContext = _httpContextAccessor.HttpContext;            
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();
            return userAgent != null ? GetBrowser(userAgent) : "Unknown Browser";
            //return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown User-Agent";
        }

        public string GetCurrentUserId()
        {
            //return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? "Anonymous";
            return (_httpContextAccessor?.HttpContext?.Items["UserId"] as int?).GetValueOrDefault().ToString();
        } 

        public int GetUserId()
        {
            //var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //return int.TryParse(userId, out _) ? userId : "0";
             var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;            
            if (int.TryParse(userId, out int parsedUserId))
            {
                return parsedUserId;
            }
            return 0;
        }
        public string GetUserName()
        {
            //var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            //return userName ?? "Anonymous";
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
            {             
                return "Anonymous";
            }
            return userName;
        }
        public string GetUserOS()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();
            return userAgent != null ? GetOperatingSystem(userAgent) : "Unknown OS";
            
        }
        private string GetOperatingSystem(string userAgent)
        {
            if (userAgent.Contains("Windows")) return "Windows";
            if (userAgent.Contains("Mac OS")) return "Mac OS";
            if (userAgent.Contains("Linux")) return "Linux";
            if (userAgent.Contains("Android")) return "Android";
            if (userAgent.Contains("iPhone")) return "iOS";
            return "Unknown OS";
        }

        private string GetBrowser(string userAgent)
        {
            if (userAgent.Contains("Firefox")) return "Firefox";
            if (userAgent.Contains("Chrome")) return "Chrome";
            if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) return "Safari";
            if (userAgent.Contains("Edge")) return "Edge";
            if (userAgent.Contains("Opera") || userAgent.Contains("OPR")) return "Opera";
            return "Unknown Browser";
        }
           public int? GetCompanyId()
          {
              var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("CompanyId")?.Value;
        
              if (int.TryParse(claim, out int companyId))
                  return companyId;
        
              return null;
          }
           public string GetGroupcode()
          {
              var GroupCode = _httpContextAccessor.HttpContext?.User?.FindFirst("GroupCode")?.Value;
        
        
              return GroupCode;
          }
          public int GetEntityId()
            {
              var EntityId = _httpContextAccessor.HttpContext?.User?.FindFirst("EntityId")?.Value;
        
        
              return EntityId != null ? Convert.ToInt32(EntityId) : 0;
          }
            public int? GetUnitId()
          {
              var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("UnitId")?.Value;
        
              if (int.TryParse(claim, out int unitId))
                  return unitId;
        
              return null;
          }
      
    }    
}


