namespace PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster
{
    public interface ITnCTemplateCodeGenerator
    {
          // Generates a TemplateCode like "PO-00001" using the TransactionType ShortName as prefix.
          Task<string> GenerateAsync(int transactionTypeId, CancellationToken ct = default);
    }
}