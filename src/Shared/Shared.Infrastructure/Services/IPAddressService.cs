using System.Net;
using System.Security.Claims;
using Contracts.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.Services;

internal sealed class IPAddressService : IIPAddressService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IPAddressService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetSystemIPAddress()
    {
        string ipAddress = "127.0.0.1";
        try
        {
            ipAddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily is System.Net.Sockets.AddressFamily.InterNetwork)?
                .ToString() ?? ipAddress;
        }
        catch { }
        return ipAddress;
    }

    public string GetUserBrowserDetails(string userAgent)
    {
        string os = ExtractOS(userAgent);
        string systemName = Environment.MachineName;
        string browserAndVersion = ExtractBrowserAndVersion(userAgent);
        string ipAddress = GetSystemIPAddress();
        return $"{os}/{systemName}/{browserAndVersion}/{ipAddress}";
    }

    private static string ExtractOS(string userAgent)
    {
        if (userAgent.Contains("PostmanRuntime")) return "PostmanOS";
        if (userAgent.Contains("Windows NT")) return "WinNT";
        if (userAgent.Contains("Mac OS X")) return "MacOS";
        if (userAgent.Contains("Linux")) return "Linux";
        if (userAgent.Contains("Android")) return "Android";
        if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
        return "UnknownOS";
    }

    private static string ExtractBrowserAndVersion(string userAgent)
    {
        if (userAgent.Contains("PostmanRuntime"))
        {
            var versionMatch = System.Text.RegularExpressions.Regex.Match(userAgent, @"PostmanRuntime/([\d\.]+)");
            return versionMatch.Success ? $"Postman/{versionMatch.Groups[1].Value}" : "Postman/Unknown";
        }

        var match = System.Text.RegularExpressions.Regex.Match(userAgent,
            @"(Chrome|Firefox|Safari|Edge|MSIE|Trident)[/ ]([\d\.]+)");

        if (match.Success)
        {
            string browser = match.Groups[1].Value;
            string version = match.Groups[2].Value;
            if (browser is "Trident") { browser = "IE"; version = "11.0"; }
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
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
        return userAgent != null ? GetBrowser(userAgent) : "Unknown Browser";
    }

    public string GetCurrentUserId()
    {
        return (_httpContextAccessor?.HttpContext?.Items["UserId"] as int?).GetValueOrDefault().ToString();
    }

    public int GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userId, out int parsedUserId) ? parsedUserId : 0;
    }

    public string GetUserName()
    {
        var ctx = _httpContextAccessor.HttpContext;
        // context.Items["UserName"] is set by TokenValidationMiddleware after JWT validation
        if (ctx?.Items["UserName"] is string name && !string.IsNullOrEmpty(name))
            return name;

        var userName = ctx?.User?.Identity?.Name
                    ?? ctx?.User?.FindFirst(ClaimTypes.Name)?.Value;
        return string.IsNullOrEmpty(userName) ? "Anonymous" : userName;
    }

    public string GetUserOS()
    {
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
        return userAgent != null ? GetOperatingSystem(userAgent) : "Unknown OS";
    }

    private static string GetOperatingSystem(string userAgent)
    {
        if (userAgent.Contains("Windows")) return "Windows";
        if (userAgent.Contains("Mac OS")) return "Mac OS";
        if (userAgent.Contains("Linux")) return "Linux";
        if (userAgent.Contains("Android")) return "Android";
        if (userAgent.Contains("iPhone")) return "iOS";
        return "Unknown OS";
    }

    private static string GetBrowser(string userAgent)
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
        if (claim == null) return null;
        return int.TryParse(claim, out int companyId) ? companyId : null;
    }

    public string GetGroupCode()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("GroupCode")?.Value ?? string.Empty;
    }

    public int GetEntityId()
    {
        var entityId = _httpContextAccessor.HttpContext?.User?.FindFirst("EntityId")?.Value;
        return entityId != null ? Convert.ToInt32(entityId) : 0;
    }

    public int? GetUnitId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("UnitId")?.Value;
        if (claim == null) return null;
        return int.TryParse(claim, out int unitId) ? unitId : null;
    }

    public int GetUnitTypeId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("UnitTypeId")?.Value;
        return int.TryParse(claim, out int unitTypeId) ? unitTypeId : 0;
    }

    public string GetUnitTypeName()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("UnitTypeName")?.Value ?? string.Empty;
    }

    public string GetOldUnitId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("OldUnitId")?.Value ?? string.Empty;
    }

    public int? GetPartyId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("PartyId")?.Value;
        if (claim == null) return null;
        return int.TryParse(claim, out int partyId) && partyId > 0 ? partyId : null;
    }

    public int? GetEmpId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("EmpId")?.Value;
        if (claim == null) return null;
        return int.TryParse(claim, out int empId) && empId > 0 ? empId : null;
    }

    public int? GetDivisionId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("DivisionId")?.Value;
        if (claim == null) return null;
        return int.TryParse(claim, out int divisionId) && divisionId > 0 ? divisionId : null;
    }
}
