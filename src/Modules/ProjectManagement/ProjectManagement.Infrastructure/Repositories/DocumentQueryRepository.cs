using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IProjectMaster;
using Core.Domain.Common;
using Dapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Infrastructure.Data;

namespace ProjectManagement.Infrastructure.Repositories
{
    public class DocumentQueryRepository : IUploadDocumentQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ApplicationDbContext _applicationDbContext;

        public DocumentQueryRepository(ApplicationDbContext applicationDbContext, IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            _applicationDbContext = applicationDbContext;
            
        }

             public async Task<string> GetDocumentDirectoryAsync()
        {
            const string query = @"
            SELECT Description            
            FROM Project.MiscTypeMaster             
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  IsDeleted=0 and IsActive=1
            ORDER BY ID DESC";
            var parameters = new { MiscTypeCode = MiscEnumEntity.DocumentPath };
            var result = await _dbConnection.QueryAsync<string>(query, parameters);
            return result.FirstOrDefault();
        }
        public async Task<bool> DeleteFileDetailsDocumentAsync(int id,  int projectId,string fileName)
        {
            var entity = await _applicationDbContext.ProjectDocument
                .FirstOrDefaultAsync(x => x.Id == id  && x.FileName == fileName);

            if (entity == null)
                return false;

            _applicationDbContext.ProjectDocument.Remove(entity);
            await _applicationDbContext.SaveChangesAsync();
            return true;

        }
        public async Task<string> GetBaseDirectoryAsync()
        {
            const string query = @"
               SELECT TOP(1) LTRIM(RTRIM(Description)) AS BaseDirectory
                FROM Project.MiscTypeMaster WHERE IsDeleted = 0   AND IsActive  = 1
                AND LTRIM(RTRIM(MiscTypeCode)) = @Code;";

            var parameters = new { poDocument = MiscEnumEntity.DocumentPath };
            var result = await _dbConnection.QueryFirstOrDefaultAsync<string>(query, new { Code = parameters });
            return result ?? string.Empty;
        }
        public async Task<IReadOnlyCollection<int>> GetUploadDocumentIdsAsync(int poId)
        {
            // Guard invalid id
            if (poId <= 0) return Array.Empty<int>();

            // Guard null/ disposed context or unset DbSet
            var set = _applicationDbContext?.ProjectDocument;
            if (set is null) return Array.Empty<int>();

            try
            {      
                var ids = await set
                    .AsNoTracking()
                    .Where(d => d != null  && d.DocumentId > 0)
                    .Select(d => d.DocumentId)
                    .Distinct()
                    .ToListAsync();

                // ToListAsync never returns null, but be extra safe
                return ids ?? (IReadOnlyCollection<int>)Array.Empty<int>();
            }
            catch
            {
                // If anything goes wrong, don't crash the pipeline — just report "none"
                return Array.Empty<int>();
            }
        }

    }
}