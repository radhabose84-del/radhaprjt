using System.Security.Claims;
using Core.Domain.Common;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces
{
    public interface IJwtTokenHelper
    {
        string GenerateToken(string? username,int userid,string Mobile,string EmailId,string IsFirstTimeUser,int EntityId,string GroupCode,int CompanyId,int DivisionId,int UnitId,string OldUnitId, out string jti);
        ClaimsPrincipal ValidateToken(string token);
    }
}