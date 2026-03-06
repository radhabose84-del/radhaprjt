using System.Data;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetNotificationTemplateAutoComplete;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace  BackgroundService.Infrastructure.Repositories.Notification.NotificationTemplate
{
    public class NotificationTemplateQueryRepository : INotificationTemplateQueryRepository
    {
        private readonly IDbConnection _dbConnection;            

        public NotificationTemplateQueryRepository([FromKeyedServices("Notification")] IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;            
        }

        public async Task<NotificationTemplateDto> GetByIdAsync(int Id)
        {            
            const string query = @" select 
                    NC.Id, NotificationTypeId, NotificationConfigId,SubjectTemplate,HeaderTemplate,BodyTemplate,FooterTemplate,LanguageCode,NCF.ModuleName,MM.Code ChannelName, NC.IsActive, 
                    NC.IsDeleted, NC.CreatedBy, NC.CreatedDate, NC.CreatedByName, NC.CreatedIP, NC.ModifiedBy, NC.ModifiedDate, NC.ModifiedByName, NC.ModifiedIP
                    FROM  AppNotification.NotificationTemplate NC
                    INNER JOIN AppNotification.NotificationConfig NCF on NCF.Id=NC.NotificationConfigId                    
                    INNER JOIN AppData.MiscMaster  MM on MM.id=NC.NotificationTypeId
                    WHERE NC.Id = @Id AND NC.IsDeleted = 0";

            var NotificationTemplate = await _dbConnection.QueryFirstOrDefaultAsync<NotificationTemplateDto>(query, new { Id });
            return NotificationTemplate;
        }

        public async Task<List<NotificationTemplateAutoCompleteDto>> GetNotificationTemplateAutoCompleteAsync(string searchPattern)
        {            
            searchPattern = searchPattern ?? string.Empty;
            const string query = @"
            SELECT NC.Id, NCF.ModuleName,MM.Code ChannelName
                    FROM  AppNotification.NotificationTemplate NC
                    INNER JOIN AppNotification.NotificationConfig NCF on NCF.Id=NC.NotificationConfigId                    
                    INNER JOIN AppData.MiscMaster  MM on MM.id=NC.NotificationTypeId
                    WHERE NC.IsDeleted = 0   AND NCF.ModuleName LIKE @SearchPattern";            
            var parameters = new
            {                
                SearchPattern = $"%{searchPattern}%"
            };
            var NotificationTemplate = await _dbConnection.QueryAsync<NotificationTemplateAutoCompleteDto>(query, parameters);
            return NotificationTemplate.ToList();
        }

        public async Task<(IEnumerable<dynamic>, int)> GetAllNotificationTemplateAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            
            var query = $$"""
            DECLARE @TotalCount INT;
            SELECT @TotalCount = COUNT(*) 
            FROM AppNotification.NotificationTemplate
            WHERE  IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ModuleName LIKE @Search)")}};

            SELECT 
                NC.Id, NotificationTypeId, NotificationConfigId,SubjectTemplate,HeaderTemplate,BodyTemplate,FooterTemplate,LanguageCode,NCF.ModuleName,MM.Code ChannelName, NC.IsActive, 
                NC.IsDeleted, NC.CreatedBy, NC.CreatedDate, NC.CreatedByName, NC.CreatedIP, NC.ModifiedBy, NC.ModifiedDate, NC.ModifiedByName, NC.ModifiedIP,NCF.ModuleName
                FROM  AppNotification.NotificationTemplate NC
                INNER JOIN AppNotification.NotificationConfig NCF on NCF.Id=NC.NotificationConfigId                    
                INNER JOIN AppData.MiscMaster  MM on MM.id=NC.NotificationTypeId
                WHERE NC.IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ModuleName LIKE @Search )")}}
            ORDER BY Id desc
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {                
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            var NotificationTemplate = await _dbConnection.QueryMultipleAsync(query, parameters);
            var NotificationTemplateList = (await NotificationTemplate.ReadAsync<NotificationTemplateDto>()).ToList();
            int totalCount = (await NotificationTemplate.ReadFirstAsync<int>());
            return (NotificationTemplateList, totalCount);
        }
        public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
                    SELECT 1 
                    FROM AppNotification.NotificationEventRule
                    WHERE TemplateId = @Id AND IsDeleted = 0";
            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });
            var NotificationTemplateExists = await multi.ReadFirstOrDefaultAsync<int?>();
            return NotificationTemplateExists.HasValue;
        }
        public async Task<bool> NotFoundAsync(int Id)
        {
            var query = "SELECT COUNT(1) FROM AppNotification.NotificationTemplate WHERE Id = @Id AND IsDeleted = 0";             
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = Id });
            return count > 0;
        }   
    }
}