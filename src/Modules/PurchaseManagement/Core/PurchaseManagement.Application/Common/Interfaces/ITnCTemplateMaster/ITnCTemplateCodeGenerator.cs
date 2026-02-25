namespace PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster
{
    public interface ITnCTemplateCodeGenerator
    {
          Task<string> GenerateAsync(int templateTypeId, string templateName, CancellationToken ct = default);
    }
}