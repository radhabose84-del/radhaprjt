using GateEntryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GateEntryManagement.Presentation.Validation.Common
{
    public class MaxLengthProvider : IMaxLengthProvider
    {
        private readonly ApplicationDbContext _dbContext;

        public MaxLengthProvider(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int? GetMaxLength<TEntity>(string propertyName)
        {
            var entityType = _dbContext.Model.FindEntityType(typeof(TEntity));
            if (entityType == null) return null;

            var property = entityType.FindProperty(propertyName);
            if (property == null) return null;

            var columnType = property.GetColumnType();
            if (string.IsNullOrWhiteSpace(columnType)) return null;

            // Parse varchar(n) or nvarchar(n)
            var match = System.Text.RegularExpressions.Regex.Match(columnType, @"\((\d+)\)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var maxLength))
                return maxLength;

            return property.GetMaxLength();
        }
    }
}
