using System.Security.Claims;

namespace UserManagement.Application.Common.Interfaces
{
    public interface IJwtTokenHelper
    {
        string GenerateToken(string? username,int userid,string Mobile,string EmailId,string IsFirstTimeUser,int EntityId,string GroupCode,int CompanyId,int DivisionId,int UnitId,string OldUnitId,string FirstName,string LastName,int? PartyId, out string jti);
        ClaimsPrincipal ValidateToken(string token);
    }
}