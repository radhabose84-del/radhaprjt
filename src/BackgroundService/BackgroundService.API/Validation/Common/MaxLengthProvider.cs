using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BackgroundService.API.Validation.Common
{
    public class MaxLengthProvider
    {
        public int? GetMaxLength<T>(string propertyName) where T : class
        {
            var property = typeof(T).GetProperty(propertyName);

            if (property == null)
                throw new InvalidOperationException($"Property {propertyName} not found in {typeof(T).Name}");

            // First, check for [MaxLength]
            var maxLengthAttr = property.GetCustomAttribute<MaxLengthAttribute>();
            if (maxLengthAttr != null)
                return maxLengthAttr.Length;

            // Check for [StringLength]
            var stringLengthAttr = property.GetCustomAttribute<StringLengthAttribute>();
            if (stringLengthAttr != null)
                return stringLengthAttr.MaximumLength;

            // If not found, return null
            return null;
        }
    }
}
