namespace Contracts.Interfaces;

public interface IIPAddressService
{
    string GetSystemIPAddress();
    string GetUserIPAddress();
    string GetUserAgent();
    string GetCurrentUserId();
    int    GetUserId();
    string GetUserName();
    string GetUserOS();
    string GetUserBrowserDetails(string userAgent);
    int?   GetCompanyId();
    string GetGroupCode();
    int    GetEntityId();
    int?   GetUnitId();
    int    GetUnitTypeId();
    string GetUnitTypeName();
    string GetOldUnitId();
    int?   GetPartyId();
    int?   GetEmpId();
    int?   GetDivisionId();

    // Comma-joined role names the current user holds, captured for the statutory audit trail
    // (US-GL02-09). Default returns null so implementations that don't surface roles (design-time
    // stubs, etc.) need no change; the real IPAddressService reads the role claims from the JWT.
    string? GetUserRole() => null;
}
