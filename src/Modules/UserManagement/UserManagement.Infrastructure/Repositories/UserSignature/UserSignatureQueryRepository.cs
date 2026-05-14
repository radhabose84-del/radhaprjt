using System.Data;
using Dapper;
using UserManagement.Application.Common.Interfaces.IUserSignature;

namespace UserManagement.Infrastructure.Repositories.UserSignature
{
    public class UserSignatureQueryRepository : IUserSignatureQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public UserSignatureQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<UserManagement.Domain.Entities.UserSignature>, int)> GetAllUserSignatureAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $$"""
                SELECT
                    us.Id, us.UserId,
                    us.FileName, us.OriginalFileName, us.FilePath, us.FileType, us.FileSize,
                    us.IsActive, us.IsDeleted,
                    us.CreatedBy, us.CreatedAt, us.CreatedByName, us.CreatedIP,
                    us.ModifiedBy, us.ModifiedAt, us.ModifiedByName, us.ModifiedIP,
                    u.FirstName, u.LastName, u.EmailId
                FROM AppData.UserSignature us
                INNER JOIN AppSecurity.Users u ON us.UserId = u.UserId
                WHERE us.IsDeleted = 0
                {{(string.IsNullOrEmpty(searchTerm) ? "" : "AND (u.FirstName LIKE @Search OR u.LastName LIKE @Search OR u.EmailId LIKE @Search OR us.FileName LIKE @Search OR us.OriginalFileName LIKE @Search)")}}
                ORDER BY us.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                """;

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var rows = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.UserSignature, UserManagement.Domain.Entities.User, UserManagement.Domain.Entities.UserSignature>(
                query,
                (signature, user) =>
                {
                    user.UserId = signature.UserId;
                    signature.User = user;
                    return signature;
                },
                parameters,
                splitOn: "FirstName");

            var list = rows.ToList();

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>($$"""
                SELECT COUNT(*)
                FROM AppData.UserSignature us
                INNER JOIN AppSecurity.Users u ON us.UserId = u.UserId
                WHERE us.IsDeleted = 0
                {{(string.IsNullOrEmpty(searchTerm) ? "" : "AND (u.FirstName LIKE @Search OR u.LastName LIKE @Search OR u.EmailId LIKE @Search OR us.FileName LIKE @Search OR us.OriginalFileName LIKE @Search)")}};
                """, new { Search = $"%{searchTerm}%" });

            return (list, totalCount);
        }

        public async Task<UserManagement.Domain.Entities.UserSignature?> GetUserSignatureByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    us.Id, us.UserId,
                    us.FileName, us.OriginalFileName, us.FilePath, us.FileType, us.FileSize,
                    us.IsActive, us.IsDeleted,
                    us.CreatedBy, us.CreatedAt, us.CreatedByName, us.CreatedIP,
                    us.ModifiedBy, us.ModifiedAt, us.ModifiedByName, us.ModifiedIP,
                    u.FirstName, u.LastName, u.EmailId
                FROM AppData.UserSignature us
                INNER JOIN AppSecurity.Users u ON us.UserId = u.UserId
                WHERE us.Id = @Id AND us.IsDeleted = 0;";

            var rows = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.UserSignature, UserManagement.Domain.Entities.User, UserManagement.Domain.Entities.UserSignature>(
                sql,
                (signature, user) =>
                {
                    user.UserId = signature.UserId;
                    signature.User = user;
                    return signature;
                },
                new { Id = id },
                splitOn: "FirstName");

            return rows.FirstOrDefault();
        }

        public async Task<UserManagement.Domain.Entities.UserSignature?> GetUserSignatureByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT
                    us.Id, us.UserId,
                    us.FileName, us.OriginalFileName, us.FilePath, us.FileType, us.FileSize,
                    us.IsActive, us.IsDeleted,
                    us.CreatedBy, us.CreatedAt, us.CreatedByName, us.CreatedIP,
                    us.ModifiedBy, us.ModifiedAt, us.ModifiedByName, us.ModifiedIP,
                    u.FirstName, u.LastName, u.EmailId
                FROM AppData.UserSignature us
                INNER JOIN AppSecurity.Users u ON us.UserId = u.UserId
                WHERE us.UserId = @UserId AND us.IsActive = 1 AND us.IsDeleted = 0;";

            var rows = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.UserSignature, UserManagement.Domain.Entities.User, UserManagement.Domain.Entities.UserSignature>(
                sql,
                (signature, user) =>
                {
                    user.UserId = signature.UserId;
                    signature.User = user;
                    return signature;
                },
                new { UserId = userId },
                splitOn: "FirstName");

            return rows.FirstOrDefault();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM AppData.UserSignature
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 0 ELSE 1 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM AppSecurity.Users
                    WHERE UserId = @UserId AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { UserId = userId });
        }

        public async Task<bool> UserHasSignatureAsync(int userId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM AppData.UserSignature
                    WHERE UserId = @UserId AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { UserId = userId });
        }
    }
}
