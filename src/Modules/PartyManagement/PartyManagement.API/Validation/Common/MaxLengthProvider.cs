using Microsoft.EntityFrameworkCore.Metadata;
using PartyManagement.Infrastructure.Data;

namespace PartyManagement.API.Validation.Common
{
    public class MaxLengthProvider 
    {
        private readonly IModel _model;

        public MaxLengthProvider(ApplicationDbContext  dbContext)
        {
            _model = dbContext.Model;
        }   

        public int? GetMaxLength<T>(string propertyName) where T : class
        {
            var entityType = _model.FindEntityType(typeof(T));

            if (entityType is null)
            {
                throw new InvalidOperationException($"Entity type {typeof(T).Name} not found in the model.");
            }

            var property = entityType.FindProperty(propertyName);

            if (property is null)
            {
                throw new InvalidOperationException($"Property {propertyName} not found in entity type {typeof(T).Name}.");
            }

            // Retrieve the column type from annotations
            var columnType = property.GetAnnotations()
                                     .FirstOrDefault(a => a.Name is "Relational:ColumnType")?.Value?.ToString();

            if (string.IsNullOrEmpty(columnType))
            {
                return null;
            }

            // Extract the max length from the column type (e.g., "varchar(50)")
            var maxLength = columnType.StartsWith("varchar(")
                ? int.Parse(columnType.Substring(8, columnType.Length - 9)) // Extract value inside parentheses
                : (int?)null;

            return maxLength;
        }
    }
}
