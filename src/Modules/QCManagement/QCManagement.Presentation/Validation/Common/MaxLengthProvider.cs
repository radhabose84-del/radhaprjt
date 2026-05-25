using QCManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Metadata;

namespace QCManagement.Presentation.Validation.Common
{
    public class MaxLengthProvider : IMaxLengthProvider
    {
        private readonly IModel? _model;

        public MaxLengthProvider(ApplicationDbContext dbContext)
        {
            _model = dbContext?.Model;
        }

        public int? GetMaxLength<TEntity>(string propertyName)
        {
            if (_model is null) return null;

            var entityType = _model.FindEntityType(typeof(TEntity));
            if (entityType is null) return null;

            var property = entityType.FindProperty(propertyName);
            if (property is null) return null;

            var columnType = property.GetAnnotations()
                .FirstOrDefault(a => a.Name is "Relational:ColumnType")?.Value?.ToString();

            if (string.IsNullOrWhiteSpace(columnType)) return null;

            var match = System.Text.RegularExpressions.Regex.Match(columnType, @"\((\d+)\)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var maxLength))
                return maxLength;

            return property.GetMaxLength();
        }
    }
}
