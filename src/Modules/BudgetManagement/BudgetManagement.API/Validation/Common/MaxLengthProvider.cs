using BudgetManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq;

namespace BudgetManagement.API.Validation.Common
{
    public class MaxLengthProvider : IMaxLengthProvider
    {
        private readonly IModel _model;

        public MaxLengthProvider(ApplicationDbContext dbContext)
        {
            _model = dbContext.Model;
        }

        public int? GetMaxLength<T>(string propertyName) where T : class
        {
            var entityType = _model.FindEntityType(typeof(T));
            if (entityType is null)
                throw new InvalidOperationException($"Entity type {typeof(T).Name} not found in the model.");

            var property = entityType.FindProperty(propertyName);
            if (property is null)
                throw new InvalidOperationException($"Property {propertyName} not found in entity type {typeof(T).Name}.");

            var columnType = property.GetAnnotations()
                .FirstOrDefault(a => a.Name is "Relational:ColumnType")?.Value?.ToString();

            if (string.IsNullOrEmpty(columnType))
                return null;

            // Example: varchar(50)
            var lower = columnType.ToLowerInvariant();
            if (lower.StartsWith("varchar(") && lower.EndsWith(")"))
            {
                var inside = lower.Substring("varchar(".Length, lower.Length - "varchar(".Length - 1);
                if (int.TryParse(inside, out var len))
                    return len;
            }

            return null;
        }
    }
}
