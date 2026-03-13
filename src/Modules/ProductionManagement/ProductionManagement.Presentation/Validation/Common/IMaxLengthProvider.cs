namespace ProductionManagement.Presentation.Validation.Common
{
    public interface IMaxLengthProvider
    {
        int? GetMaxLength<TEntity>(string propertyName);
    }
}
