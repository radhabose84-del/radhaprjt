using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Core.Application.Common.Interfaces;
using Core.Domain.Common;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace UserManagement.Infrastructure.Repositories
{
    public class JwtTokenHelper : IJwtTokenHelper
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ITimeZoneService _timeZoneService;
        public JwtTokenHelper(IOptions<JwtSettings> jwtSettings, ITimeZoneService timeZoneService)
        {          
            if (jwtSettings == null || jwtSettings.Value == null)
            {
                throw new ArgumentNullException(nameof(jwtSettings), "JWT settings are not configured.");
            }

            _jwtSettings = jwtSettings.Value;
            _timeZoneService = timeZoneService;

            if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            {
                throw new ArgumentException("JWT SecretKey must be configured.", nameof(_jwtSettings.SecretKey));
            }

            if (string.IsNullOrWhiteSpace(_jwtSettings.EncryptionKey))
            {
                throw new ArgumentException("JWT EncryptionKey must be configured.", nameof(_jwtSettings.EncryptionKey));
            }

            if (Encoding.UTF8.GetBytes(_jwtSettings.SecretKey).Length < 32)
            {
                throw new ArgumentException("JWT SecretKey must be at least 32 bytes long.", nameof(_jwtSettings.SecretKey));
            }
            var encryptionKeyBytes = Convert.FromBase64String(_jwtSettings.EncryptionKey);            
           if (encryptionKeyBytes.Length  != 32)
            {
                throw new ArgumentException("JWT EncryptionKey must be exactly 32 bytes long.", nameof(_jwtSettings.EncryptionKey));
            } 
        }

      public string GenerateToken(string? username,int userid,string Mobile,string EmailId,string IsFirstTimeUser,int EntityId,string GroupCode,int CompanyId,int DivisionId,int UnitId,string? OldUnitId,string? FirstName,string? LastName,  out string jti)       
        {
            jti = Guid.NewGuid().ToString();            
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId); 

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.NameId, userid.ToString()), 
                new Claim("Mobile", Mobile.ToString()),
                new Claim("EmailId", EmailId.ToString()),
                new Claim("CompanyId", CompanyId.ToString()),
                new Claim("DivisionId", DivisionId.ToString()),
                new Claim("UnitId", UnitId.ToString()),
                 new Claim("IsFirstTimeUser", IsFirstTimeUser.ToString()),
                new Claim("EntityId", EntityId.ToString()),
                new Claim("GroupCode", GroupCode.ToString()),
                new Claim("OldUnitId", OldUnitId.ToString()),
                new Claim("FirstName", FirstName.ToString()),
                new Claim("LastName", LastName.ToString()),
                new Claim("TimeStamp", DateTimeOffset.UtcNow.ToString("o")),
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(currentTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // foreach (var role in roles)
            // {
            //     claims.Add(new Claim(ClaimTypes.Role, role));
            // }
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // Decode Base64 EncryptionKey
            var encryptionKeyBytes = Convert.FromBase64String(_jwtSettings.EncryptionKey);
            var encryptionKey = new SymmetricSecurityKey(encryptionKeyBytes);
            // var encryptingCredentials = new EncryptingCredentials(
            //     encryptionKey,
            //     SecurityAlgorithms.Aes256KW,
            //     SecurityAlgorithms.Aes256CbcHmacSha512
            // );

            var tokenDescriptor = new SecurityTokenDescriptor
            {
            Subject = new ClaimsIdentity(claims),
            Expires = currentTime.AddMinutes(_jwtSettings.ExpiryMinutes),
            SigningCredentials = signingCredentials,
            // EncryptingCredentials = encryptingCredentials,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var encryptedToken = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(encryptedToken);        

        }

        
        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);            
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var encryptionKey = new SymmetricSecurityKey(Convert.FromBase64String(_jwtSettings.EncryptionKey));

           /*  var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ClockSkew = TimeSpan.Zero // Prevent delayed expiration
            }; */
             var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, // Validate the signing key
                IssuerSigningKey = signingKey,  // Use the signing key
                TokenDecryptionKey = encryptionKey, // Use the encryption key
                ValidateIssuer = true,          // Validate the issuer
                ValidIssuer = _jwtSettings.Issuer, // Match the issuer
                ValidateAudience = true,        // Validate the audience
                ValidAudience = _jwtSettings.Audience, // Match the audience
                ClockSkew = TimeSpan.Zero       // Prevent leeway for expiration
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                 // Optional: Check additional claims like "jti"
                var jti = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(jti))
                {
                    throw new SecurityTokenException("Missing or invalid JWT ID.");
                }
                return principal;
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Invalid token.", ex);
            }
        }
      public ClaimsPrincipal ValidateAndDecryptToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var encryptionKey = new SymmetricSecurityKey(Convert.FromBase64String(_jwtSettings.EncryptionKey)); // Decode Base64 EncryptionKey

            // Set token validation parameters
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, // Validate the signing key
                IssuerSigningKey = signingKey,  // Key used for signing
                TokenDecryptionKey = encryptionKey, // Key used for decrypting the token
                ValidateIssuer = true,          // Validate the issuer
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,        // Validate the audience
                ValidAudience = _jwtSettings.Audience,
                ClockSkew = TimeSpan.Zero       // Prevent leeway for expiration
            };

            try
            {
                // Attempt to validate and decrypt the token
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Additional debug: Check token type
                if (validatedToken is not JwtSecurityToken jwtToken || jwtToken.Header.Alg != SecurityAlgorithms.RsaOAEP || jwtToken.Header.Enc != SecurityAlgorithms.Aes256CbcHmacSha512)
                {
                    throw new SecurityTokenException("Invalid token encryption or signing algorithm.");
                }

                return principal; // Return claims principal on successful validation
            }
            catch (Exception ex)
            {                
                throw new SecurityTokenException("Invalid or decryption failed for token.", ex);
            }
        }
       

    }
}
