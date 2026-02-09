namespace BudgetManagement.API.Validation.Common
{
    public interface IMaxLengthProvider
    {
        int? GetMaxLength<T>(string propertyName) where T : class;
    }
}
